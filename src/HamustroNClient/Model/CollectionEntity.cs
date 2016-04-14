using System.Collections.Generic;

namespace HamustroNClient.Model
{
    public class CollectionEntity
    {
        public string DeviceId { get; set; }

        public string ClientId { get; set; }

        public string Session { get; set; }

        public string SystemVersion { get; set; }

        public string ProductVersion { get; set; }

        public string System { get; set; }

        public string ProductGitHash { get; set; }

        public List<PayloadEntity> Payloads { get; set; }
    }    
}
