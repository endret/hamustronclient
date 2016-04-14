using HamustroNClient.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HamustroNClient.Implementation
{
    public class ExternalIpResolver : IIpResolver
    {
        private const string Url = "https://api.ipify.org";

        public async Task<string> GetIp()
        {
            using (var httpClient = new HttpClient())
            {
                return await httpClient.GetStringAsync(Url);
            }
        }       
    }
}
