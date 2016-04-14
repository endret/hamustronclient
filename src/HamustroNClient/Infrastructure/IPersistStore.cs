using System;
using System.Collections.Generic;
using HamustroNClient.Model;
using System.Threading.Tasks;

namespace HamustroNClient.Infrastructure
{
    public interface IPersistentStorage
    {
        DateTime? LastSyncDateTime { get; set; }

        Task Add(EventCollection eventCollection);

        Task<IEnumerable<EventCollection>> Get();

        Task Delete(EventCollection eventCollection);
    }
}