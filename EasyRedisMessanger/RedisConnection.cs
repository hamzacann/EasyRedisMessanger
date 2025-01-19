using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyRedis
{
    /// <summary>
    /// Defines a contract for publishing messages.
    /// </summary>
    public interface IPublisher
    {
        /// <summary>
        /// Publishes a message to a specified channel.
        /// </summary>
        /// <param name="channelName">The channel name.</param>
        /// <param name="message">The message content.</param>
        /// <param name="cacheMessage">Indicates whether the message should be cached.</param>
        /// <param name="cacheTime">The cache duration.</param>
        void Publish(string channelName, string message, bool cacheMessage = false, TimeSpan? cacheTime = null);

        /// <summary>
        /// Asynchronously publishes a message to a specified channel.
        /// </summary>
        /// <param name="channelName">The channel name.</param>
        /// <param name="message">The message content.</param>
        /// <param name="cacheMessage">Indicates whether the message should be cached.</param>
        /// <param name="cacheTime">The cache duration.</param>
        Task PublishAsync(string channelName, string message, bool cacheMessage = false, TimeSpan? cacheTime = null);
    }

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

    /// <summary>
    /// Defines a contract for managing message caching.
    /// </summary>
    public interface ICacheManager
    {
        /// <summary>
        /// Caches a message.
        /// </summary>
        /// <param name="channelName">The channel name.</param>
        /// <param name="message">The message content.</param>
        /// <param name="cacheDuration">The cache duration.</param>
        void CacheMessage(string channelName, string message, TimeSpan? cacheDuration);

        /// <summary>
        /// Asynchronously caches a message.
        /// </summary>
        /// <param name="channelName">The channel name.</param>
        /// <param name="message">The message content.</param>
        /// <param name="cacheDuration">The cache duration.</param>
        Task CacheMessageAsync(string channelName, string message, TimeSpan? cacheDuration);

        /// <summary>
        /// Gets historical messages from the cache.
        /// </summary>
        /// <param name="channelName">The channel name.</param>
        /// <returns>The list of historical messages.</returns>
        List<string> GetHistoricalMessages(string channelName);
    }

    /// <summary>
    /// Implementation of <see cref="ICacheManager"/> using Redis.
    /// </summary>
    public class RedisCacheManager : ICacheManager
    {
        private readonly IConnectionMultiplexer _connection;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisCacheManager"/> class.
        /// </summary>
        /// <param name="connection">The Redis connection multiplexer.</param>
        public RedisCacheManager(IConnectionMultiplexer connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        /// <inheritdoc/>
        public void CacheMessage(string channelName, string message, TimeSpan? cacheDuration)
        {
            var database = _connection.GetDatabase();
            var expiry = cacheDuration ?? TimeSpan.FromSeconds(120);
            database.ListRightPush(channelName, message);
            database.KeyExpire(channelName, expiry);
        }

        /// <inheritdoc/>
        public async Task CacheMessageAsync(string channelName, string message, TimeSpan? cacheDuration)
        {
            var database = _connection.GetDatabase();
            var expiry = cacheDuration ?? TimeSpan.FromSeconds(120);
            await database.ListRightPushAsync(channelName, message);
            await database.KeyExpireAsync(channelName, expiry);
        }

        /// <inheritdoc/>
        public List<string> GetHistoricalMessages(string channelName)
        {
            var database = _connection.GetDatabase();
            var messages = database.ListRange(channelName);
            return messages.Select(m => m.ToString()).ToList();
        }
    }

    /// <summary>
    /// Event arguments for message received events.
    /// </summary>
    public class MessageReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the received message.
        /// </summary>
        public ChannelMessage Message { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageReceivedEventArgs"/> class.
        /// </summary>
        /// <param name="message">The received message.</param>
        public MessageReceivedEventArgs(ChannelMessage message)
        {
            Message = message;
        }
    }

    /// <summary>
    /// Defines a contract for listening to messages.
    /// </summary>
    public interface IListener
    {
        /// <summary>
        /// Occurs when a message is received.
        /// </summary>
        event EventHandler<MessageReceivedEventArgs> MessageReceived;

        /// <summary>
        /// Starts listening for messages on a specified channel.
        /// </summary>
        /// <param name="channelName">The channel name.</param>
        /// <param name="includeHistory">Indicates whether to include historical messages.</param>
        void StartListening(string channelName, bool includeHistory = false);
    }

    /// <summary>
    /// Implementation of <see cref="IListener"/> using Redis.
    /// </summary>
    public class RedisListener : IListener
    {
        private readonly IConnectionMultiplexer _connection;
        private readonly ICacheManager _cacheManager;
        private readonly ISubscriber _subscriber;

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
                    OnMessageReceived(new ChannelMessage(channelName, message));
                }
            }

            _subscriber.Subscribe(channelName, (channel, message) =>
            {
                OnMessageReceived(new ChannelMessage(channel, message));
            });
        }

        /// <summary>
        /// Raises the <see cref="MessageReceived"/> event.
        /// </summary>
        /// <param name="message">The received message.</param>
        protected virtual void OnMessageReceived(ChannelMessage message)
        {
            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message));
        }
    }
}
