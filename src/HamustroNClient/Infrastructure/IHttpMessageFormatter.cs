using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using HamustroNClient.Model;
using System.Net.Http;
using HamustroNClient.Core;
using HamustroNClient.Security;

namespace HamustroNClient.Infrastructure
{
    public interface IHttpMessageFormatter
    {
        HttpRequestMessage GetRequestMessage(EventCollection eventCollection, string sharedKey);
    }

    public class GoogleProtoFormatter : IHttpMessageFormatter
    {
        public HttpRequestMessage GetRequestMessage(EventCollection eventCollection, string sharedKey)
        {
            var result = new HttpRequestMessage();

            var content = ConvertToByteArray(eventCollection);

            result.Content = new ByteArrayContent(content);

            var ts = DateTime.UtcNow.GetEpochUtc();

            result.Headers.Add("X-Hamustro-Time", ts.ToString());

            result.Headers.Add("X-Hamustro-Signature", CalculateCollectorSignature(ts, content, sharedKey));

            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/protobuf");

            return result;
        }

        private static byte[] ConvertToByteArray(EventCollection eventCollection)
        {
            var c = eventCollection.Collection;

            var cbuilder = new Collection.Builder();

            cbuilder.DeviceId = c.DeviceId;

            cbuilder.ClientId = c.ClientId;

            cbuilder.Session = c.Session;

            cbuilder.SystemVersion = c.SystemVersion;

            cbuilder.ProductVersion = c.ProductVersion;

            cbuilder.System = c.System;

            cbuilder.ProductGitHash = c.ProductGitHash;

            foreach (var p in c.Payloads)
            {
                var pBuilder = new Payload.Builder();

                pBuilder.At = p.At;

                pBuilder.Event = p.Event;

                pBuilder.Nr = p.Nr;

                pBuilder.UserId = p.UserId;

                pBuilder.Ip = p.Ip;

                pBuilder.Parameters = p.Parameters;

                pBuilder.IsTesting = p.IsTesting;

                var payload = pBuilder.Build();

                cbuilder.AddPayloads(payload);
            }

            return cbuilder.Build().ToByteArray();
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

            var h = HashUtil.HashSha256(Encoding.UTF8.GetBytes(sb.ToString()));

            return Convert.ToBase64String(h);
        }

    }
}
