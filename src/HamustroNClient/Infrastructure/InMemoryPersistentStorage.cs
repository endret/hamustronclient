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
        private static readonly object Lck = new object();

        private readonly IDictionary<string, SessionCollection> _storage = new Dictionary<string, SessionCollection>();

        public DateTime? LastSyncDateTime { get; set; }
        
        public Task Add(SessionCollection sessionCollection)
        {
            lock (Lck)
            {
                if (_storage.ContainsKey(sessionCollection.SessionId))
                {
                    foreach (var ce in sessionCollection.Collection.Payloads)
                    {
                        _storage[sessionCollection.SessionId].Collection.Payloads.Add(ce);
                    }
                }
                else
                {
                    _storage.Add(sessionCollection.SessionId, sessionCollection);
                }
            }

            return Task.FromResult(0);
        }

        public Task<IEnumerable<SessionCollection>> Get()
        {
            return Task.FromResult(_storage.Values.AsEnumerable());
        }

        public Task Delete(SessionCollection sessionCollection)
        {
            lock (Lck)
            {
                _storage.Remove(sessionCollection.SessionId);
            }

            return Task.FromResult(0);
        }
    }
}