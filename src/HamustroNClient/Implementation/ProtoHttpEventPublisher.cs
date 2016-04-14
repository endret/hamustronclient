using HamustroNClient.Infrastructure;

namespace HamustroNClient.Implementation
{
    public class ProtoHttpEventPublisher : HttpEventPublisher
    {
        public ProtoHttpEventPublisher(string collectorUrl) : base(collectorUrl, new GoogleProtoFormatter())
        {
        }
    }
}