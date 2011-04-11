using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Anytao.Common.Caching;

namespace Anytao.Common.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            AtCache<int, string> cache = new AtCache<int, string>();
            // cache.Expiration = 10;

            cache.Add(1, "1");
            cache.Set(1, "2", 3);

            System.Threading.Thread.Sleep(1);

            string value;
            if (cache.TryGetValue(1, out value))
            {

            }
        }
    }
}
