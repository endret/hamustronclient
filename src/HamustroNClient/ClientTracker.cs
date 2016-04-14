using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using HamustroNClient.Core;
using HamustroNClient.Infrastructure;
using HamustroNClient.Model;
using HamustroNClient.Security;
using System.Collections.Generic;

namespace HamustroNClient
{
    public class ClientTracker
    {
        private const string ClientVersion = "0.1";

        private readonly IPersistentStorage _persistentStorage;
        private readonly IEventPublisher _eventPublisher;

        private static readonly Encoding StringEncoding = Encoding.UTF8;

        private static object _lckSessionSerial = new object();

        private readonly string _collectorUrl;
        private readonly string _sharedSecretKey;
        private readonly string _deviceIdHash;
        private readonly string _clientId;
        private readonly string _systemVersion;
        private readonly string _productVersion;
        private readonly string _system;
        private readonly string _productGitHash;
        private readonly int _queueSize;
        private readonly int _queueRetentionMinutes;

        private string _sessionId;
        private uint _sessionSerial;

        [Obsolete]
        private Uri CollectorUri
        {
            get
            {
                return new Uri(new Uri(this._collectorUrl), "/api/v1/track");
            }
        }

        /// <summary>
        /// Initialize ClientTracker instance
        /// </summary>
        /// <param name="collectorUrl">required, set from config</param>
        /// <param name="sharedSecretKey">required, set from config</param>
        /// <param name="deviceId">required</param>
        /// <param name="clientId">required</param>
        /// <param name="systemVersion">required</param>
        /// <param name="productVersion"></param>
        /// <param name="system"></param>
        /// <param name="productGitHash"></param>
        /// <param name="queueSize">default: 20</param>
        /// <param name="queueRetentionMinutes">default: 1440 (minutes = 24 hours)</param>
        public ClientTracker(
            string collectorUrl,
            string sharedSecretKey,
            string deviceId,
            string clientId,
            string systemVersion,
            string productVersion,
            string system,
            string productGitHash,
            int queueSize = 20,
            int queueRetentionMinutes = 1440
            )
        {
            collectorUrl.Check(s => !string.IsNullOrWhiteSpace(s), "collectorUrl");
            sharedSecretKey.Check(s => !string.IsNullOrWhiteSpace(s), "sharedSecretKey");
            deviceId.Check(s => !string.IsNullOrWhiteSpace(s), "deviceId");
            clientId.Check(s => !string.IsNullOrWhiteSpace(s), "clientId");
            systemVersion.Check(s => !string.IsNullOrWhiteSpace(s), "systemVersion");

            this._collectorUrl = collectorUrl;

            this._sharedSecretKey = sharedSecretKey;

            this._deviceIdHash = HashUtil.HashSha256ToString(StringEncoding.GetBytes(deviceId));

            this._clientId = clientId;

            this._systemVersion = systemVersion;

            this._productVersion = productVersion;

            this._system = system;

            this._productGitHash = productGitHash;

            this._queueSize = Math.Max(0, queueSize);

            this._queueRetentionMinutes = Math.Max(0, queueRetentionMinutes);

            // TODO move into constructor as dependency
            this._persistentStorage = new InMemoryPersistentStorage();

            // TODO move into constructor as dependency
            this._eventPublisher = new ProtoHttpEventPublisher(collectorUrl);  
        }
                
        /// <summary>
        /// It will generate pre-populated information for new events so it should not be calculated on adding each event.
        /// </summary>
        public void GenerateSession()
        {
            // TODO maintenance storage!

            // Generated as md5hex(device_id + ":" + client_id + ":" + system_version + ":" product_version)

            var raw = string.Format("{0}:{1}:{2}:{3}",
                this._deviceIdHash,
                this._clientId,
                this._systemVersion,
                this._productVersion);

            this._sessionId = HashUtil.HashMd5ToString(StringEncoding.GetBytes(raw));

            this._sessionSerial = 0u;
        }

        /// <summary>
        /// Loading information not sent events from persistent storage.
        /// </summary>
        public void LoadCollections()
        {
            // TODO ask Bitu (why it's neccessary)
            throw new NotImplementedException();
        }

        /// <summary>
        /// events per session
        /// </summary>
        public async Task<int> LoadNumberPerSession()
        {
            var repo = await this._persistentStorage.Get();

            var r = repo.SingleOrDefault(c => c.SessionId == this._sessionId);

            if (r == null)
            {
                return 0;
            }

            return r.Collection.Payloads.Count();
        }

        /// <summary>
        /// timestamp for events sent last time
        /// </summary>
        public DateTime? LoadLastSyncTime()
        {
            return _persistentStorage.LastSyncDateTime;
        }

        public async Task TrackEvent(string eventName, uint userId, string parameters, bool isTest = false)
        {
            eventName.Check(s => !string.IsNullOrWhiteSpace(s), "eventName");

            var payload = new PayloadEntity
            {
                At = DateTime.UtcNow.GetEpochUtc(),
                Event = eventName,
                Nr = IncrementSerial(ref this._sessionSerial),
                UserId = userId,
                Ip = ResolveIp(),
                Parameters = parameters,
                IsTesting = isTest
            };

            var ce = this.CreateCollectionEntity(new List<PayloadEntity> { payload });

            await this._persistentStorage.Add(new EventCollection
            {
                SessionId = this._sessionId,
                Collection = ce
            });

            // Trigger publish mechanism
            await this.SendItemsToCollector();
        }

        private string ResolveIp()
        {
            return "127.0.0.1";
        }
        
        private async Task SendItemsToCollector()
        {
            if (_persistentStorage.LastSyncDateTime < DateTime.UtcNow.AddMinutes(-this._queueRetentionMinutes))
            {
                return;
            }

            var repo = await _persistentStorage.Get();

            if (repo.Sum(g => g.Collection.Payloads.Count) <= this._queueSize)
            {
                return;
            }

            // TODO paralell run
            foreach (var eventCollection in repo)
            {
                var isSuccess = await this._eventPublisher.Send(eventCollection, this._sharedSecretKey);

                if (isSuccess)
                {
                    await _persistentStorage.Delete(eventCollection);
                }
            }
        }

        private CollectionEntity CreateCollectionEntity(List<PayloadEntity> payloads)
        {
            var result = new CollectionEntity
            {
                ClientId = this._clientId,

                DeviceId = this._deviceIdHash,

                Session = this._sessionId,

                SystemVersion = this._systemVersion,

                ProductVersion = this._productVersion,

                System = this._system,

                ProductGitHash = this._productGitHash,

                Payloads = payloads
            };

            return result;
        }

        private static string CalculateCollectorSignature(ulong epochTimestamp, byte[] requestBody, string sharedKey)
        {
            // X-Hamustro-Signature: base64(sha256(X-Hamustro-Time + "|" + md5hex(request.body) + "|" + t.shared_secret_key))

            var sb = new StringBuilder();

            sb.Append(epochTimestamp);

            sb.Append('|');

            sb.Append(HashUtil.HashMd5ToString(requestBody));

            sb.Append('|');

            sb.Append(sharedKey);

            var h = HashUtil.HashSha256(StringEncoding.GetBytes(sb.ToString()));

            return Convert.ToBase64String(h);
        }

        private static uint IncrementSerial(ref uint i)
        {
            lock (_lckSessionSerial)
            {
                if (i == uint.MaxValue)
                {
                    i = 1U;
                }
                else
                {
                    i++;
                }

                return i;
            }
        }
    }
}
