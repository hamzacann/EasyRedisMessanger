using StackExchange.Redis;

namespace EasyRedis
{
    public class RedisConnection
    {
        private readonly IConnectionMultiplexer _connection;

        public RedisConnection(IConnectionMultiplexer connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }
        public IDatabase GetDatabase()
        {
            return _connection.GetDatabase();
        }

        public ISubscriber GetSubscriber()
        {
            return _connection.GetSubscriber();
        }

        public void Publish(string channelName, string message, bool cacheMessage = false, TimeSpan? cacheTime = null)
        {
            var publisher = _connection.GetSubscriber();
            publisher.Publish(channelName, message);
            if (cacheMessage)
            {
                CacheMessage(channelName, message, cacheTime);
            }
        }

        public async Task PublishAsync(string channelName, string message, bool cacheMessage = false, TimeSpan? cacheTime = null)
        {
            var publisher = _connection.GetSubscriber();
            await publisher.PublishAsync(channelName, message);
            if (cacheMessage)
            {
                await CacheMessageAsync(channelName, message, cacheTime);
            }
        }

        public void Listen(string channelName, Action<ChannelMessage> onMessageReceived, bool includeHistory = false)
        {
            var subscriber = _connection.GetSubscriber();
            if (includeHistory)
            {
                var messages = GetHistoricalMessages(channelName);
                foreach (var message in messages)
                {
                    onMessageReceived(new ChannelMessage(channelName, message));
                }
            }
            subscriber.Subscribe(channelName, (channel, message) =>
            {
                onMessageReceived(new ChannelMessage(channel, message));
            });
        }

        public async Task ListenAsync(string channelName, Func<ChannelMessage, Task> onMessageReceived, bool includeHistory = false)
        {
            var subscriber = _connection.GetSubscriber();
            if (includeHistory)
            {
                var messages = GetHistoricalMessages(channelName);
                foreach (var message in messages)
                {
                    var channelMessage = new ChannelMessage(channelName, message);
                    await onMessageReceived(channelMessage);
                }
            }
            await subscriber.SubscribeAsync(channelName, async (channel, message) =>
            {
                var channelMessage = new ChannelMessage(channel.ToString(), message);
                await onMessageReceived(channelMessage);
            });
        }

        private void CacheMessage(string channelName, string message, TimeSpan? cacheDuration)
        {
            var database = _connection.GetDatabase();
            var expiry = cacheDuration ?? TimeSpan.FromSeconds(120);//default time to keep message in cache
            database.ListRightPush(channelName, message);//leftpush for lifo
            database.KeyExpire(channelName, expiry);
        }
        private async Task CacheMessageAsync(string channelName, string message, TimeSpan? cacheDuration)
        {
            var database = _connection.GetDatabase();
            var expiry = cacheDuration ?? TimeSpan.FromSeconds(120);//default time to keep message in cache
            await database.ListRightPushAsync(channelName, message);//leftpush for lifo
            await database.KeyExpireAsync(channelName, expiry);
        }

        private List<string> GetHistoricalMessages(string channelName)
        {
            var database = _connection.GetDatabase();
            var messages = database.ListRange(channelName);
            return messages.Select(m => m.ToString()).ToList();
        }
    }

}
