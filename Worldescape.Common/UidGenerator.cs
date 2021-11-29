using System;

namespace Worldescape.Common
{
    public static class UidGenerator
    {
        static private readonly DateTime DateSeed = DateTime.Parse("2013/01/01");

        static public int New(int prefix = 1)
        {
            var value = (long)((DateTime.UtcNow - DateSeed).TotalMilliseconds + (prefix * 100000000000));

            var returnValue = Math.Abs((int)value);
            return returnValue;
        }
    }
}
