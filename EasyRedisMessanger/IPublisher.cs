using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyRedisMessanger
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
}
