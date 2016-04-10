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

namespace HamustroNClient
{
    public class ClientTracker
    {
        private const string ClientVersion = "0.1";

        private readonly IPersistentStorage _persistentStorage;
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

            this._persistentStorage = DefaultPersistentStorageFactory();

            this._persistentStorage.LastSyncDateTime = DateTime.UtcNow;
        }

        // TODO move into constructor as dependency
        public static Func<IPersistentStorage> DefaultPersistentStorageFactory = () => new InMemoryPersistentStorage();

        /// <summary>
        /// It will generate pre-populated information for new events so it should not be calculated on adding each event.
        /// </summary>
        public void GenerateSession()
        {
            // TODO maintenance storage!

            // Generated as md5hex(device_id + ":" + client_id + ":" + system_version + ":" product_version)

            var raw = String.Format("{0}:{1}:{2}:{3}",
                this._deviceIdHash,
                this._clientId,
                this._systemVersion,
                this._productVersion);

            this._sessionId = HashUtil.HashMd5ToString(StringEncoding.GetBytes(raw));

            this._sessionSerial = 1u;
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
        public int LoadNumberPerSession()
        {
            var cr = this._persistentStorage.Get().FirstOrDefault(c => c.Id == this._sessionId);

            if (cr == null)
            {
                return 0;
            }

            return cr.Collection.PayloadsList.Count();
        }

        /// <summary>
        /// timestamp for events sent last time
        /// </summary>
        public DateTime LoadLastSyncTime()
        {
            return _persistentStorage.LastSyncDateTime;
        }

        public async Task TrackEvent(string eventName, int userId, string parameters, bool isTest = false)
        {
            eventName.Check(s => !string.IsNullOrWhiteSpace(s), "eventName");

            var pb = Payload.CreateBuilder();

            pb.At = DateTime.UtcNow.GetEpochUtc();

            pb.Event = eventName;

            pb.Nr = IncrementSerial(ref this._sessionSerial);

            pb.UserId = (uint)userId;

            // TODO implement ip resolver logic
            pb.Ip = "127.0.0.1";

            pb.Parameters = parameters;

            pb.IsTesting = isTest;

            var cb = this.CreateCollection();

            cb.AddPayloads(pb.Build());

            this._persistentStorage.Add(new CollectionEntity(this._sessionId)
            {
                Collection = cb.Build()
            });


            // Trigger sending mechanism
            await this.SendItemsToCollector();
        }

        private async Task SendItemsToCollector()
        {
            if (_persistentStorage.LastSyncDateTime < DateTime.UtcNow.AddMinutes(-this._queueRetentionMinutes))
            {
                return;
            }

            if (_persistentStorage.Get().Sum(g => g.Collection.PayloadsCount) >= this._queueSize)
            {
                return;
            }

            foreach (var collectionEntity in this._persistentStorage.Get())
            {
                // TODO split collection by payloadcount

                var ts = DateTime.UtcNow.GetEpochUtc();

                var content = collectionEntity.Collection.ToByteArray();

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("hamustroNClient", ClientVersion));

                    httpClient.DefaultRequestHeaders.Add("Content-type", "application/protobuf");

                    httpClient.DefaultRequestHeaders.Add("X-Hamustro-Time", ts.ToString());

                    var sig = CalculateCollectorSignature(ts, content, this._sharedSecretKey);

                    httpClient.DefaultRequestHeaders.Add("X-Hamustro-Signature", sig);

                    var r = await httpClient.PostAsync(this.CollectorUri, new ByteArrayContent(content));

                    if (r.StatusCode == HttpStatusCode.OK)
                    {
                        _persistentStorage.Delete(collectionEntity);
                        _persistentStorage.LastSyncDateTime = DateTime.UtcNow;
                    }
                }               
            }
        }

        private Collection.Builder CreateCollection()
        {
            var cb = Collection.CreateBuilder();

            cb.ClientId = this._clientId;

            cb.DeviceId = this._deviceIdHash;

            cb.Session = this._sessionId;

            cb.SystemVersion = this._systemVersion;

            cb.ProductVersion = this._productVersion;

            cb.System = this._system;

            cb.ProductGitHash = this._productGitHash;

            return cb;
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
