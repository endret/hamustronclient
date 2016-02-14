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

        private readonly IDictionary<string, CollectionEntity> _storage = new Dictionary<string, CollectionEntity>();

        public DateTime LastSyncDateTime { get; set; }
        
        public void Add(CollectionEntity collectionEntity)
        {
            lock (lck)
            {
                if (_storage[collectionEntity.Id] != null)
                {
                    foreach (var ce in collectionEntity.Collection.PayloadsList)
                    {
                        _storage[collectionEntity.Id].Collection.PayloadsList.Add(ce);
                    }
                }
                else
                {
                    _storage.Add(collectionEntity.Id, collectionEntity);
                }
            }
        }

        public IEnumerable<CollectionEntity> Get()
        {
            return _storage.Values;
        }

        public void Delete(CollectionEntity collectionEntity)
        {
            lock (lck)
            {
                _storage.Remove(collectionEntity.Id);
            }
        }
    }
}