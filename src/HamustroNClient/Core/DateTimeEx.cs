using System;
using System.Net.NetworkInformation;

namespace HamustroNClient.Core
{
    public static class DateTimeEx
    {
        public static ulong GetEpochUtc(this DateTime dt)
        {
            var t = dt - new DateTime(1970, 1, 1);
            
            return (ulong) t.TotalSeconds;
        }
    }
}