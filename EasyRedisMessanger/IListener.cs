using EasyRedis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyRedisMessanger
{
    /// <summary>
    /// Implements the <see cref="IListener"/> interface using Redis and handles message receiving and key expiration events.
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

        /// <summary>
        /// Stops listening for messages on a specified channel.
        /// </summary>
        /// <param name="channelName">The channel name.</param>
        /// <param name="clearHistory">Indicates whether to clear historical messages.</param>
        void StopListening(string channelName, bool clearHistory = true);
    }
}
