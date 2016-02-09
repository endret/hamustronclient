using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamustroNClient.Core
{
    public static class ParamCheck
    {
        public static void Check<T>(this T s, Func<T, bool> validator)
        {
            if (!validator(s))
            {
                throw new ArgumentException("Invalid argument!");
            }
        }
        
        public static void Check<T>(this T s, Func<T, bool> validator, string paramName)
        {
            if (!validator(s))
            {
                throw new ArgumentException("Invalid argument!", paramName);
            }
        }

        public static void Check<T, TException>(this T s, Func<T, bool> validator, string paramName, TException exception) where TException :Exception
        {
            if (!validator(s))
            {
                throw exception;
            }
        }
    }
}
