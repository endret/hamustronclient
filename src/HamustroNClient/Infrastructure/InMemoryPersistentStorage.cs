using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using HamustroNClient.Model;

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
        
        public void Add(EventCollection eventCollection)
        {
            lock (lck)
            {
                if (_storage[eventCollection.SessionId] != null)
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
        }

        public IEnumerable<EventCollection> Get()
        {
            return _storage.Values;
        }

        public void Delete(EventCollection eventCollection)
        {
            lock (lck)
            {
                _storage.Remove(eventCollection.SessionId);
            }
        }
    }
}