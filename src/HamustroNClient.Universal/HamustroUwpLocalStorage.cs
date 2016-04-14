using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HamustroNClient.Model;
using Windows.Storage;
using HamustroNClient.Infrastructure;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Threading;

namespace HamustroNClient.Universal
{   
    public class HamustroUwpLocalStorage : IPersistentStorage
    {
        private const string CollectionFolderName = "coll";
        private static readonly StorageFolder RootFolder = ApplicationData.Current.LocalFolder;
        private static readonly object LastSyncDateTimeLock = new object();
        private static readonly ReaderWriterLockSlim CollectionLock = new ReaderWriterLockSlim();

        public DateTime? LastSyncDateTime
        {
            get
            {
                lock (LastSyncDateTimeLock)
                {
                    return this.GetItemFromStorage<DateTime?>(RootFolder, nameof(LastSyncDateTime)).Result; 
                }
            }

            set
            {
                lock (LastSyncDateTimeLock)
                {
                    this.SetItemToStorage<DateTime?>(RootFolder, nameof(LastSyncDateTime), value).Wait(); 
                }
            }
        }

        public async Task Add(EventCollection eventCollection)
        {
            CollectionLock.EnterWriteLock();

            try
            {
                var cf = await GetCollectionsRoot();

                var collection = await GetItemFromStorage<EventCollection>(cf, eventCollection.SessionId);

                if (collection == default(EventCollection))
                {
                    await SetItemToStorage(cf, eventCollection.SessionId, eventCollection);
                }
            }
            finally
            {
                CollectionLock.ExitWriteLock();
            }
        }

        public async Task Delete(EventCollection eventCollection)
        {
            CollectionLock.EnterWriteLock();

            try
            {
                var cf = await GetCollectionsRoot();

                var f = await cf.GetFileAsync(eventCollection.SessionId);

                if (f != null)
                {
                    await f.DeleteAsync();
                }
            }
            finally
            {
                CollectionLock.ExitWriteLock();
            }
        }

        public async Task<IEnumerable<EventCollection>> Get()
        {
            CollectionLock.EnterReadLock();

            try
            {
                var cf = await GetCollectionsRoot();

                var files = await cf.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.OrderByDate);

                return await Task.WhenAll(files.Select(async f => await DeSerializeFile<EventCollection>(f)));
            }
            finally
            {
                CollectionLock.ExitReadLock();
            }            
        }
               
        private async Task<StorageFolder> GetCollectionsRoot()
        {
            return await RootFolder.CreateFolderAsync(CollectionFolderName, CreationCollisionOption.OpenIfExists);
        } 
        
        private async Task<T> GetItemFromStorage<T>(StorageFolder folder, string fileName)
        {
            var file = await folder.GetFileAsync(fileName);

            if (file == null)
            {
                return default(T);
            }

            return await DeSerializeFile<T>(file);
        }

        private static async Task<T> DeSerializeFile<T>(StorageFile file)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));

            using (var fileContent = await file.OpenReadAsync())
            {
                var rawObject = serializer.ReadObject(fileContent.AsStreamForRead());

                return (T)rawObject;
            }
        }

        private async Task SetItemToStorage<T>(StorageFolder folder, string fileName, T item)
        {
            var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
              
            var serializer = new DataContractJsonSerializer(typeof(T));

            using (var fileContent = await file.OpenStreamForWriteAsync())
            {
                serializer.WriteObject(fileContent, item);
            }
        }
    }
}
