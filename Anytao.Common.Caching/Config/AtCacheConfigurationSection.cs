using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Anytao.Common.Caching.Config
{
    /// <summary>
    /// AtCache configuration section
    /// </summary>
    /// <code>https://github.com/anytao/atcache</code>
    public class AtCacheConfigurationSection : ConfigurationSection, IAtCacheConfiguration
    {
        #region IAtCacheConfiguration Members

        //[IntegerValidator(MinValue = 0)]
        [ConfigurationProperty("expiry", IsRequired = true, DefaultValue = 30)]
        public int Expiration
        {
            get
            {
                return (int)this["expiry"];
            }
            set
            {
                this["expiry"] = value;
            }
        }

        #endregion
    }
}
