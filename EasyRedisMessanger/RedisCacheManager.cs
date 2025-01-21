using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyRedisMessanger
{
    /// <summary>
    /// Implementation of <see cref="ICacheManager"/> using Redis.
    /// </summary>
    public class RedisCacheManager : ICacheManager
    {
        private readonly IConnectionMultiplexer _connection;
        public readonly IDatabase _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisCacheManager"/> class.
        /// </summary>
        /// <param name="connection">The Redis connection multiplexer.</param>
        public RedisCacheManager(IConnectionMultiplexer connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _db = _connection.GetDatabase();
        }

        /// <inheritdoc />
        public void CacheMessage(string channelName, string message, TimeSpan? cacheDuration)
        {
            var expiry = cacheDuration ?? TimeSpan.FromSeconds(120);
            var key = $"{channelName}+{Guid.NewGuid()}";// Create unique key
            _db.StringSet(key, message, expiry); // Set the message with expire time
        }

        /// <inheritdoc />
        public async Task CacheMessageAsync(string channelName, string message, TimeSpan? cacheDuration)
        {
            var expiry = cacheDuration ?? TimeSpan.FromSeconds(120);
            var key = $"{channelName}+{Guid.NewGuid()}";// Create unique key
            await _db.StringSetAsync(key, message, expiry); // Set the message with expire time
        }

        /// <inheritdoc />
        public List<string> GetHistoricalMessages(string channelName)
        {
            var server = _db.Multiplexer.GetServer(_db.IdentifyEndpoint());
            var keys = server.Keys(pattern: $"{channelName}+*");
            var messages = new List<string>();
            foreach (var key in keys)
            {
                var message = _db.StringGet(key);
                messages.Add(message);
            }
            return messages;
        }

        /// <inheritdoc />
        public event EventHandler<KeyExpiredEventArgs> KeyExpired;

        /// <summary>
        /// Subscribes to Redis key expiration events for the listening channel.
        /// </summary>
        public void SubscribeToExpireEvents(string channelName, ISubscriber sub)
        {
            sub.Subscribe("__keyevent@0__:expired", (channel, message) =>
            {
                var expiredKey = (string)message;
                if(!expiredKey.StartsWith(channelName))
                {
                    return;
                }
                var value = _db.StringGet(expiredKey);
                KeyExpired?.Invoke(this, new KeyExpiredEventArgs(expiredKey, value));
            });
        }

        /// <summary>
        /// Unsubscribes to Redis key expiration events for the listener.
        /// </summary>
        public void UnsubscribeFromExpireEvents(ISubscriber sub)
        {
            sub.Unsubscribe("__keyevent@0__:expired");
        }

    }
}
