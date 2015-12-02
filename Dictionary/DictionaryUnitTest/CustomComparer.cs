using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DictionaryUnitTest
{
    class CustomComparer : IEqualityComparer<int>
    {
        private IEqualityComparer<int> defaultComparer = EqualityComparer<int>.Default;
        
        public bool Equals(int x, int y)
        {
            x %= 100;
            y %= 100;
            return defaultComparer.Equals(x, y);
        }

        public int GetHashCode(int obj)
        {
            return defaultComparer.GetHashCode(obj % 100);
        }
    }
}
