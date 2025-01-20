using EasyRedis;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyRedisMessanger
{
    /// <summary>
    /// Implementation of <see cref="IPublisher"/> using Redis.
    /// </summary>
    public class RedisPublisher : IPublisher
    {
        private readonly IConnectionMultiplexer _connection;
        private readonly ICacheManager _cacheManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisPublisher"/> class.
        /// </summary>
        /// <param name="connection">The Redis connection multiplexer.</param>
        /// <param name="cacheManager">The cache manager.</param>
        public RedisPublisher(IConnectionMultiplexer connection, ICacheManager cacheManager)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
        }

        /// <inheritdoc/>
        public void Publish(string channelName, string message, bool cacheMessage = false, TimeSpan? cacheTime = null)
        {
            var publisher = _connection.GetSubscriber();
            publisher.Publish(channelName, message);

            if (cacheMessage)
            {
                _cacheManager.CacheMessage(channelName, message, cacheTime);
            }
        }

        /// <inheritdoc/>
        public async Task PublishAsync(string channelName, string message, bool cacheMessage = false, TimeSpan? cacheTime = null)
        {
            var publisher = _connection.GetSubscriber();
            await publisher.PublishAsync(channelName, message);

            if (cacheMessage)
            {
                await _cacheManager.CacheMessageAsync(channelName, message, cacheTime);
            }
        }
    }
}
