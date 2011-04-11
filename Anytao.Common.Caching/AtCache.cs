using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using Anytao.Common.Caching.Config;
using System.Threading;

namespace Anytao.Common.Caching
{
    /// <summary>
    /// A light cache framework
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <remarks>anytao / 09/10/2009</remarks>
    /// <code>https://github.com/anytao/atcache</code>
    public class AtCache<TKey, TValue> : ICache<TKey, TValue>
    {
        #region Ctor

        public AtCache()
        {
            this.config = ConfigurationManager.GetSection("atcache") as AtCacheConfigurationSection;
            this.expiration = config.Expiration;
        }

        #endregion

        #region Properties

        public int Count
        {
            get
            {
                rwLocker.EnterReadLock();

                try
                {
                    return container.Count;
                }
                finally
                {
                    rwLocker.ExitReadLock();
                }
            }
        }

        public List<TValue> Items
        {
            get
            {
                if (items == null)
                {
                    items = new List<TValue>();

                    foreach (var item in container)
                    {
                        items.Add(item.Value.Value);
                    }
                }

                return items;
            }
        }

        /// <summary>
        /// Seconds of whole cache expire
        /// </summary>
        public int Expiration
        {
            get
            {
                if (expiration < 0)
                {
                    expiration = config.Expiration;
                }

                return expiration;
            }

            set
            {
                this.expiration = value;
            }
        }

        #endregion

        #region Event

        public event EventHandler<CacheItemEventArgs<TKey, TValue>> ItemGetted;

        public event EventHandler<CacheItemEventArgs<TKey, TValue>> ItemExpired;

        #endregion

        #region Public Methods

        /// <summary>
        /// Add value to container, if existing, do not it.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(TKey key, TValue value)
        {
            rwLocker.EnterWriteLock();

            try
            {
                if (!container.ContainsKey(key))
                {
                    container.Add(key, new CacheItem<TKey, TValue>(key, value));
                }
            }
            finally
            {
                rwLocker.ExitWriteLock();
            }
        }

        /// <summary>
        /// Set value to caching container, if existing, replace it.
        /// Expiration is forever.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set(TKey key, TValue value)
        {
            this.Set(key, value, 0);
        }

        /// <summary>
        /// Set value to caching container, if existing, replace it.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiry">Expiration in seconds</param>
        public void Set(TKey key, TValue value, int expiry)
        {
            rwLocker.EnterWriteLock();
            try
            {
                if (!container.ContainsKey(key))
                {
                    container.Add(key, new CacheItem<TKey, TValue>(key, value, expiry));
                }
                else
                {
                    container.Remove(key);
                    container.Add(key, new CacheItem<TKey, TValue>(key, value, expiry));
                }
            }
            finally
            {
                rwLocker.ExitWriteLock();
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            bool result = false;
            CacheItem<TKey, TValue> item;

            Refresh();

            rwLocker.EnterReadLock();
            try
            {
                if (container.TryGetValue(key, out item))
                {
                    OnItemGetted(this, new CacheItemEventArgs<TKey, TValue>(key, item.Value));

                    // If the Container's expire is false, but the item's expire is true,
                    // still remove it.
                    TimeSpan ts = DateTime.Now - lastRefresh;

                    if (item.IsExpired(ts.TotalSeconds))
                    {
                        container.Remove(item.Key);

                        lastRefresh = DateTime.Now;

                        value = default(TValue);
                        result = false;
                    }
                    else
                    {
                        value = item.Value;
                        result = true;
                    }

                }
                else
                {
                    value = default(TValue);
                    result = false;
                }
            }
            finally
            {
                rwLocker.ExitReadLock();
            }

            return result;
        }

        public void Clear()
        {
            if (container.Count > 0)
            {
                rwLocker.EnterWriteLock();

                try
                {
                    container.Clear();
                }
                finally
                {
                    rwLocker.ExitWriteLock();
                }
            }
        }

        public bool IsExist(TKey key)
        {
            if (container.Count <= 0)
            {
                return false;
            }

            bool result;

            rwLocker.EnterReadLock();
            try
            {
                result = container.ContainsKey(key);
            }
            finally
            {
                rwLocker.ExitReadLock();
            }

            return result;
        }

        public List<TValue> ToValueList()
        {
            List<TValue> result = new List<TValue>();

            try
            {
                rwLocker.EnterReadLock();

                if (container != null && container.Count() > 0)
                {
                    foreach (var item in container)
                    {
                        result.Add(item.Value.Value);
                    }
                }
            }
            catch (System.Exception)
            {
                rwLocker.ExitReadLock();
            }
            finally
            {
                rwLocker.ExitReadLock();
            }

            return result;
        }

        public bool GetSingleValueList(out TValue value)
        {
            rwLocker.EnterReadLock();

            try
            {
                value = default(TValue);
                if (container != null && container.Count() == 1)
                {
                    value = container.FirstOrDefault().Value.Value;
                    rwLocker.ExitReadLock();
                    return true;
                }
            }
            finally
            {
                rwLocker.ExitReadLock();
            }

            return false;
        }

        #endregion

        #region Protected Methods

        protected void Refresh()
        {
            TimeSpan ts = DateTime.Now - lastRefresh;

            rwLocker.EnterWriteLock();

            try
            {
                if (ts.TotalSeconds > this.Expiration)
                {
                    // A rough arithmetic to remove the expired item,
                    // need to a better arithmetic for item expiration
                    // and container expiration. 

                    // if the container's expire is ture, but the item's expire is false, 
                    // igore the container's expire
                    var list = container.Where(item => item.Value.IsExpired(ts.TotalSeconds)).ToList();

                    for (int i = 0; i < list.Count(); i++)
                    {
                        var item = list[i];

                        container.Remove(item.Key);

                        OnItemExpired(this, new CacheItemEventArgs<TKey, TValue>(item.Key, item.Value.Value));
                    }

                    lastRefresh = DateTime.Now;
                }
            }
            finally
            {
                rwLocker.ExitWriteLock();
            }
        }

        protected virtual void OnItemExpired(object sender, CacheItemEventArgs<TKey, TValue> e)
        {
            EventHandler<CacheItemEventArgs<TKey, TValue>> handler = ItemExpired;

            if (handler != null)
            {
                handler(sender, e);
            }
        }

        protected virtual void OnItemGetted(object sender, CacheItemEventArgs<TKey, TValue> e)
        {
            EventHandler<CacheItemEventArgs<TKey, TValue>> handler = ItemGetted;

            if (handler != null)
            {
                handler(sender, e);
            }
        }

        #endregion

        #region Private Methods

        private V ReadLock<V>(Func<V> func)
        {
            rwLocker.EnterReadLock();

            try
            {
                return func();
            }
            finally
            {
                rwLocker.ExitReadLock();
            }
        }

        private void WriteLock(Action action)
        {
            rwLocker.EnterWriteLock();

            try
            {
                action();
            }
            finally
            {
                rwLocker.ExitWriteLock();
            }
        }

        #endregion

        #region Private Members

        /// <summary>
        /// A RW lock object
        /// </summary>
        private ReaderWriterLockSlim rwLocker = new ReaderWriterLockSlim();

        /// <summary>
        /// A dictionary container
        /// </summary>
        private Dictionary<TKey, CacheItem<TKey, TValue>> container = new Dictionary<TKey, CacheItem<TKey, TValue>>();

        private int expiration;

        private DateTime lastRefresh = DateTime.Now;

        private IAtCacheConfiguration config;

        private List<TValue> items;

        #endregion
    }
}
