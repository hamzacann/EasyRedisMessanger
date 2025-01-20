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
    /// Implementation of <see cref="IListener"/> using Redis.
    /// </summary>
    public class RedisListener : IListener
    {
        private readonly IConnectionMultiplexer _connection;
        private readonly ICacheManager _cacheManager;
        private readonly ISubscriber _subscriber;
        private readonly IDatabase _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisListener"/> class.
        /// </summary>
        /// <param name="connection">The Redis connection multiplexer.</param>
        /// <param name="cacheManager">The cache manager.</param>
        public RedisListener(IConnectionMultiplexer connection, ICacheManager cacheManager)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
            _subscriber = _connection.GetSubscriber(); // Initializes the subscriber.
            _db = connection.GetDatabase();
            ((RedisCacheManager)_cacheManager).SubscribeToExpireEvents();
        }

        /// <inheritdoc/>
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        /// <inheritdoc/>
        public void StartListening(string channelName, bool includeHistory = false)
        {
            if (includeHistory)
            {
                var messages = _cacheManager.GetHistoricalMessages(channelName);
                foreach (var message in messages)
                {
                    OnMessageReceived(new EasyRedis.ChannelMessage(channelName, message));
                }
            }

            _subscriber.Subscribe(channelName, (channel, message) =>
            {
                OnMessageReceived(new EasyRedis.ChannelMessage(channel, message));
            });
        }

        /// <inheritdoc/>
        public void StopListening(string channelName, bool clearHistory = true)
        {
            _subscriber.Unsubscribe(channelName);
            if (clearHistory)
            {
                var server = _db.Multiplexer.GetServer(_db.IdentifyEndpoint());
                var keys = server.Keys(pattern: $"{channelName}+*");
                foreach (var key in keys)
                {
                    _db.KeyDelete(key);
                }
            }
        }


        /// <summary>
        /// Raises the MessageReceived event.
        /// </summary>
        /// <param name="message">The received message.</param>
        protected virtual void OnMessageReceived(EasyRedis.ChannelMessage message)
        {
            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message));
        }
    }
}
