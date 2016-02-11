using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using HamustroNClient.Core;
using HamustroNClient.Infrastructure;
using HamustroNClient.Model;
using HamustroNClient.Security;

namespace HamustroNClient
{
    public class ClientTracker
    {
        private readonly IPersistentStorage _persistentStorage;
        private static readonly Encoding StringEncoding = Encoding.UTF8;

        private readonly string _collectorUrl;
        private readonly string _sharedSecretKey;
        private readonly string _deviceId;
        private readonly string _clientId;
        private readonly string _systemVersion;
        private readonly string _productVersion;
        private readonly string _system;
        private readonly string _productGitHash;
        private readonly int _queueSize;
        private readonly int _queueRetentionMinutes;

        private string _sessionId;
        private long _sessionSerial;

        /// <summary>
        /// Initialize ClientTracker instance
        /// </summary>
        /// <param name="collectorUrl">required, set from config</param>
        /// <param name="sharedSecretKey">required, set from config</param>
        /// <param name="deviceId">required, please sha256 it.</param>
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

            this._deviceId = HashUtil.HashSha256ToString(StringEncoding.GetBytes(deviceId));

            this._clientId = clientId;

            this._systemVersion = systemVersion;

            this._productVersion = productVersion;

            this._system = system;

            this._productGitHash = productGitHash;

            this._queueSize = Math.Max(0, queueSize);

            this._queueRetentionMinutes = Math.Max(0, queueRetentionMinutes);

            this._persistentStorage = DefaultPersistentStorageFactory();
        }

        // TODO move into constructor as dependency
        public static Func<IPersistentStorage> DefaultPersistentStorageFactory = () => new InMemoryPersistentStorage();
        

        /// <summary>
        /// It will generate pre-populated information for new events so it should not be calculated on adding each event.
        /// </summary>
        public void GenerateSession()
        {
            // TODO ask should we persist these data

            // Generated as md5hex(device_id + ":" + client_id + ":" + system_version + ":" product_version)

            var raw = String.Format("{0}:{1}:{2}:{3}",
                this._deviceId,
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
            // TODO 
            throw new NotImplementedException();
        }

        /// <summary>
        /// events per session
        /// </summary>
        public void LoadNumberPerSession()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// timestamp for events sent last time
        /// </summary>
        public void LoadLastSyncTime()
        {
            throw new NotImplementedException();
        }

        public void TrackEvent(string eventName, int userId, string parameters, bool isTest = false)
        {
            eventName.Check(s => !string.IsNullOrWhiteSpace(s), "eventName");

            var pb = Payload.CreateBuilder();

            pb.At = DateTime.UtcNow.GetEpochUtc();

            pb.Event = eventName;

            pb.Nr = (uint)System.Threading.Interlocked.Increment(ref _sessionSerial);
            
            pb.UserId = (uint)userId;

            // TODO implement logic
            pb.Ip = "127.0.0.1";

            pb.Parameters = parameters;

            pb.IsTesting = isTest;

            var cb = Collection.CreateBuilder();

            cb.AddPayloads(pb.Build());

            cb.ClientId = this._clientId;

            cb.DeviceId = this._deviceId;

            cb.Session = this._sessionId;

            cb.SystemVersion = this._systemVersion;

            cb.ProductVersion = this._productVersion;

            cb.System = this._system;

            cb.ProductGitHash = this._productGitHash;


            this._persistentStorage.Add(new CollectionEntity
            {
                Collection = cb.Build()
            });


            // Trigger sending mechanism
        }

    }
}
