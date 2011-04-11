using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Anytao.Common.Caching.Config
{
    /// <summary>
    /// AtCache configuratioin
    /// </summary>
    /// <code>https://github.com/anytao/atcache</code>
    public interface IAtCacheConfiguration
    {
        /// <summary>
        /// Expiratioin for caching refreshing, the default is never expired.
        /// </summary>
        int Expiration { get; set; }
    }
}
