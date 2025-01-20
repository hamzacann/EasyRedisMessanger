using EasyRedis;

namespace EasyRedisMessanger
{
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
}
