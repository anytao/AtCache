using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Anytao.Common.Caching
{
    /// <summary>
    /// Event arguments for cache item
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <code>https://github.com/anytao/atcache</code>
    public class CacheItemEventArgs<TKey, TValue> : EventArgs<TKey, TValue>
    {
        public CacheItemEventArgs()
        { }

        public CacheItemEventArgs(TKey key, TValue value)
            : base(key, value)
        { }
    }
}
