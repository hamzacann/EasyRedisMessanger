using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyRedisMessanger
{

    /// <summary>
    /// Defines a contract for managing message caching using Redis Hashes.
    /// </summary>
    public interface ICacheManager
    {
        /// <summary>
        /// Caches a message in a Redis Hash.
        /// </summary>
        /// <param name="channelName">The name of the Hash (used as the key).</param>
        /// <param name="message">The message content to be cached.</param>
        /// <param name="cacheDuration">The duration for which the message should be cached.</param>
        void CacheMessage(string channelName, string message, TimeSpan? cacheDuration);

        /// <summary>
        /// Asynchronously caches a message in a Redis Hash.
        /// </summary>
        /// <param name="channelName">The name of the Hash (used as the key).</param>
        /// <param name="message">The message content to be cached.</param>
        /// <param name="cacheDuration">The duration for which the message should be cached.</param>
        Task CacheMessageAsync(string channelName, string message, TimeSpan? cacheDuration);

        /// <summary>
        /// Gets historical messages from the Redis Hash.
        /// </summary>
        /// <param name="channelName">The name of the Hash (used as the key).</param>
        /// <returns>A list of historical messages.</returns>
        List<string> GetHistoricalMessages(string channelName);

        /// <summary>
        /// Occurs when a key expires in Redis.
        /// </summary>
        public event EventHandler<KeyExpiredEventArgs> KeyExpired;
    }

}
