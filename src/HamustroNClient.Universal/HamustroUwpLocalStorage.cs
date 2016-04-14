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
        private const string LastSyncDateTimeFileName = nameof(LastSyncDateTimeFileName);
        private static readonly StorageFolder RootFolder = ApplicationData.Current.LocalFolder;
        private static readonly ReaderWriterLockSlim LastSyncDateTimeLock = new ReaderWriterLockSlim();
        private static readonly ReaderWriterLockSlim CollectionLock = new ReaderWriterLockSlim();
        
        public async Task SetLastSyncDateTime(DateTime dateTime)
        {
            LastSyncDateTimeLock.EnterWriteLock();

            try
            {
                await this.SetItemToStorage(RootFolder, LastSyncDateTimeFileName, dateTime);
            }
            finally
            {
                LastSyncDateTimeLock.ExitWriteLock();
            }
        }

        public async Task<DateTime?> GetLastSyncDateTime()
        {
            LastSyncDateTimeLock.EnterReadLock();

            try
            {
                return await this.GetItemFromStorage<DateTime?>(RootFolder, LastSyncDateTimeFileName);
            }
            finally
            {
                LastSyncDateTimeLock.ExitReadLock();
            }            
        }

        public async Task Add(SessionCollection eventCollection)
        {
            CollectionLock.EnterWriteLock();

            try
            {
                var cf = await GetCollectionsRoot();

                var storedCollection = await GetItemFromStorage<SessionCollection>(cf, eventCollection.SessionId);

                if (storedCollection != default(SessionCollection))
                {
                    // merge payloads
                    eventCollection.Collection.Payloads.AddRange(storedCollection.Collection.Payloads);
                }                

                await SetItemToStorage(cf, eventCollection.SessionId, eventCollection);
            }
            finally
            {
                CollectionLock.ExitWriteLock();
            }
        }

        public async Task Delete(SessionCollection eventCollection)
        {
            CollectionLock.EnterWriteLock();

            try
            {
                var cf = await GetCollectionsRoot();

                var f = await cf.TryGetItemAsync(eventCollection.SessionId);

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

        public async Task<IEnumerable<SessionCollection>> Get()
        {
            CollectionLock.EnterReadLock();

            try
            {
                var cf = await GetCollectionsRoot();

                var files = await cf.GetFilesAsync();

                return await Task.WhenAll(files.Select(async f => await DeSerializeFile<SessionCollection>(f)));
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
            var file = (StorageFile)await folder.TryGetItemAsync(fileName);

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
