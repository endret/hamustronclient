using HamustroNClient.Core;
using HamustroNClient.Model;
using HamustroNClient.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HamustroNClient.Infrastructure
{
    public interface IEventPublisher
    {
        Task<bool> Send(EventCollection eventCollection, string sharedKey);
    }

    public abstract class HttpEventPublisher : IEventPublisher
    {
        private string _collectorUrl;

        private IHttpMessageFormatter _httpMessageFormatter;

        private Uri CollectorUri
        {
            get
            {
                return new Uri(new Uri(this._collectorUrl), "/api/v1/track");
            }
        }

        protected HttpEventPublisher(string collectorUrl, IHttpMessageFormatter httpMessageFormatter )
        {
            this._collectorUrl = collectorUrl;
            this._httpMessageFormatter = httpMessageFormatter;
        }

        public async Task<bool> Send(EventCollection eventCollection, string sharedKey)
        {
            using (var httpClient = new HttpClient())
            using (var request = this._httpMessageFormatter.GetRequestMessage(eventCollection, sharedKey))
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = CollectorUri;

                var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                return response.StatusCode == System.Net.HttpStatusCode.OK;
            }            
        }        
    }

    public class ProtoHttpEventPublisher : HttpEventPublisher
    {
        public ProtoHttpEventPublisher(string collectorUrl) : base(collectorUrl, new GoogleProtoFormatter())
        {
        }
    }
}
