using System;
using System.Collections.Generic;
using HamustroNClient.Model;

namespace HamustroNClient.Infrastructure
{
    public interface IPersistentStorage
    {
        DateTime? LastSyncDateTime { get; set; }

        void Add(EventCollection eventCollection);

        IEnumerable<EventCollection> Get();

        void Delete(EventCollection eventCollection);
    }
}