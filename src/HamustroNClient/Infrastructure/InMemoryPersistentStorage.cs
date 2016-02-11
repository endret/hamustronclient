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
    internal class InMemoryPersistentStorage : IPersistentStorage
    {
        private readonly IDictionary<Guid, CollectionEntity> _storage = new ConcurrentDictionary<Guid, CollectionEntity>();

        public DateTime LastSyncDateTime { get; set; }
        public void Add(CollectionEntity collectionEntity)
        {
            _storage.Add(collectionEntity.Id, collectionEntity);
        }

        public IEnumerable<CollectionEntity> Get()
        {
            return _storage.Values;
        }

        public void Delete(CollectionEntity collectionEntity)
        {
            _storage.Remove(collectionEntity.Id);
        }
    }
}