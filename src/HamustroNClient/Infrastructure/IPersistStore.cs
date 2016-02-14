using System;
using System.Collections.Generic;
using HamustroNClient.Model;

namespace HamustroNClient.Infrastructure
{
    public interface IPersistentStorage
    {
        DateTime LastSyncDateTime { get; set; }

        void Add(Model.CollectionEntity collectionEntity);

        IEnumerable<Model.CollectionEntity> Get();

        void Delete(CollectionEntity collectionEntity);
    }
}