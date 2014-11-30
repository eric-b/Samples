using System;
using System.Linq;

namespace DisruptorSamples
{
    public static class ExtensionMethods
    {
        public static bool IsPowerOfTwo(this int x)
        {
            return x > 1 && (x & (x - 1)) == 0;
        }

        public static int NextPowerOfTwo(this int x)
        {
            if (x < 2)
                return 2;
            x--;
            x |= (x >> 1);
            x |= (x >> 2);
            x |= (x >> 4);
            x |= (x >> 8);
            x |= (x >> 16);
            return (x + 1);
        }
    }
}