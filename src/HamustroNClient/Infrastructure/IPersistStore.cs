using System;
using System.Collections.Generic;
using HamustroNClient.Model;
using System.Threading.Tasks;

namespace HamustroNClient.Infrastructure
{
    public interface IPersistentStorage
    {
        Task SetLastSyncDateTime(DateTime dateTime);

        Task<DateTime?> GetLastSyncDateTime();

        Task Add(SessionCollection sessionCollection);

        Task<IEnumerable<SessionCollection>> Get();

        Task Delete(SessionCollection sessionCollection);
    }
}