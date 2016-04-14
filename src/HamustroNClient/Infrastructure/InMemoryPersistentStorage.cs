using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using HamustroNClient.Model;
using System.Threading.Tasks;

namespace HamustroNClient.Infrastructure
{
    /// <summary>
    /// InMemory persistent storage. Using for test ONLY!
    /// </summary>
    public class InMemoryPersistentStorage : IPersistentStorage
    {
        private static object lck = new object();

        private readonly IDictionary<string, EventCollection> _storage = new Dictionary<string, EventCollection>();

        public DateTime? LastSyncDateTime { get; set; }
        
        public Task Add(EventCollection eventCollection)
        {
            lock (lck)
            {
                if (_storage.ContainsKey(eventCollection.SessionId))
                {
                    foreach (var ce in eventCollection.Collection.Payloads)
                    {
                        _storage[eventCollection.SessionId].Collection.Payloads.Add(ce);
                    }
                }
                else
                {
                    _storage.Add(eventCollection.SessionId, eventCollection);
                }
            }

            return Task.FromResult(0);
        }

        public Task<IEnumerable<EventCollection>> Get()
        {
            return Task.FromResult(_storage.Values.AsEnumerable());
        }

        public Task Delete(EventCollection eventCollection)
        {
            lock (lck)
            {
                _storage.Remove(eventCollection.SessionId);
            }

            return Task.FromResult(0);
        }
    }
}