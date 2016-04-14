using HamustroNClient.Core;
using HamustroNClient.Model;
using HamustroNClient.Security;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamustroNClient.Infrastructure
{
    public interface IEventPublisher
    {
        Task<bool> Send(SessionCollection sessionCollection, string sharedKey);
    }
}
