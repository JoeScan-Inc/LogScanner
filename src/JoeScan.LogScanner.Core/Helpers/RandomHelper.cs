using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoeScan.LogScanner.Core.Helpers
{
    public static  class RandomHelper
    {
        private static readonly Random rnd = new();
        public static int GetRandom()
        {
            return rnd.Next();
        }
        public static double GetRandomDouble()
        {
            return rnd.NextDouble();
        }
    }
}
