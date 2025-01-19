using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyRedis
{
    public class ChannelMessage
    {
        public string Channel { get; }
        public string Message { get; }

        public ChannelMessage(string channel, string message)
        {
            Channel = channel;
            Message = message;
        }
    }
}
