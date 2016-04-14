using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HamustroNClient.Model;
using System.Net.Http;

namespace HamustroNClient.Infrastructure
{
    public interface IHttpMessageFormatter
    {
        HttpRequestMessage GetRequestMessage(SessionCollection sessionCollection, string sharedKey);
    }
}
