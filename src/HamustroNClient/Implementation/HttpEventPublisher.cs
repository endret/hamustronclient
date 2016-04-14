using System;
using System.Net.Http;
using System.Threading.Tasks;
using HamustroNClient.Infrastructure;
using HamustroNClient.Model;

namespace HamustroNClient.Implementation
{
    public abstract class HttpEventPublisher : IEventPublisher
    {
        private readonly string _collectorUrl;

        private readonly IHttpMessageFormatter _httpMessageFormatter;

        private Uri CollectorUri => new Uri(new Uri(this._collectorUrl), "/api/v1/track");

        protected HttpEventPublisher(string collectorUrl, IHttpMessageFormatter httpMessageFormatter )
        {
            this._collectorUrl = collectorUrl;
            this._httpMessageFormatter = httpMessageFormatter;
        }

        public async Task<bool> Send(SessionCollection sessionCollection, string sharedKey)
        {
            using (var httpClient = new HttpClient())
            using (var request = this._httpMessageFormatter.GetRequestMessage(sessionCollection, sharedKey))
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = CollectorUri;

                var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                return response.StatusCode == System.Net.HttpStatusCode.OK;
            }            
        }        
    }
}