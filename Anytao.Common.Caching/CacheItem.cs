using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Anytao.Common.Caching
{
    /// <summary>
    /// Cache item
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <code>https://github.com/anytao/atcache</code>
    internal class CacheItem<TKey, TValue>
    {
        public CacheItem(TKey key, TValue value)
            : this(key, value, 0)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiration">
        /// The default expiration is 0, 
        /// it means the item is never expired.
        /// </param>
        public CacheItem(TKey key, TValue value, int expiration)
        {
            this.key = key;
            this.value = value;
            this.expiration = expiration;
        }

        public TKey Key { get { return this.key; } }

        public TValue Value { get { return this.value; } }
        public int Expiration { get { return this.expiration; } }

        /// <summary>
        /// The default expiration is 0, it means the item is never expired.
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        public bool IsExpired(double span)
        {
            if (this.expiration > 0)
            {
                return span - this.expiration > 0;
            }
            else
            {
                return false;
            }
        }


        private TKey key;
        private TValue value;
        private int expiration;
    }
}
