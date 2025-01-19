using System;

namespace EasyRedis
{
    /// <summary>
    /// Represents a message received from a Redis channel.
    /// </summary>
    public class ChannelMessage
    {
        /// <summary>
        /// Gets the name of the channel from which the message was received.
        /// </summary>
        public string Channel { get; }

        /// <summary>
        /// Gets the content of the message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelMessage"/> class with the specified channel name and message content.
        /// </summary>
        /// <param name="channel">The name of the channel.</param>
        /// <param name="message">The content of the message.</param>
        public ChannelMessage(string channel, string message)
        {
            Channel = channel;
            Message = message;
        }
    }
}
