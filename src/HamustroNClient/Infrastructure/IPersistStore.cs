using System;
using System.Collections.Generic;
using HamustroNClient.Model;
using System.Threading.Tasks;

namespace HamustroNClient.Infrastructure
{
    public interface IPersistentStorage
    {
        DateTime? LastSyncDateTime { get; set; }

        Task Add(SessionCollection sessionCollection);

        Task<IEnumerable<SessionCollection>> Get();

        Task Delete(SessionCollection sessionCollection);
    }
}