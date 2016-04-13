using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HamustroNClient.Model;
using Windows.Storage;
using HamustroNClient.Infrastructure;

namespace HamustroNClient.Universal
{   
    public class HamustroUwpLocalStorage : IPersistentStorage
    {
        private static readonly StorageFolder RootFolder = ApplicationData.Current.LocalFolder;
      
        public DateTime? LastSyncDateTime
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public void Add(EventCollection eventCollection)
        {
            throw new NotImplementedException();
        }

        public void Delete(EventCollection eventCollection)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<EventCollection> Get()
        {
            throw new NotImplementedException();
        }

        private async Task<CollectionEntity> GetCollections(string Id)
        {
            var r = await GetCollectionsRoot();

            var f = await r.GetFileAsync(Id.ToLowerInvariant());

            if (f == null)
            {
                return null;
            }

            throw new NotImplementedException();  
        }

        private async Task<StorageFolder> GetCollectionsRoot()
        {
            return await RootFolder.CreateFolderAsync("collections", CreationCollisionOption.OpenIfExists);
        }      
    }

}
