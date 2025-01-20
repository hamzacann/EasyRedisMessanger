using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyRedisMessanger
{
    /// <summary>
    /// Provides data for the <see cref="ICacheManager.KeyExpired"/> event.
    /// This event is raised when a key in Redis expires.
    /// </summary>
    public class KeyExpiredEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the key that expired.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Gets the value associated with the expired key.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyExpiredEventArgs"/> class.
        /// </summary>
        /// <param name="key">The key that expired.</param>
        /// <param name="value">The value associated with the expired key.</param>
        public KeyExpiredEventArgs(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}
