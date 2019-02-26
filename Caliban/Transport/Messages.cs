using System;
using System.Linq;
using Caliban.Core.Utility;

namespace Caliban.Core.Transport
{
    public enum MessageType 
    {
        GAME_CLOSE = 0x0000,
        DEBUG_LOG = 0x0001,
        REGISTER = 0x0002,
        
        GAME_WIN = 0x0003,
        GAME_LOSE = 0x0004,
        CONSUME_TREASURE = 0x0005,
        
        WATERLEVEL_SET = 0x0101,
        WATERLEVEL_GET = 0x0102,
        WATERLEVEL_ADD = 0x0103
    }

    public struct Message
    {
        public readonly MessageType Type;
        public readonly string Value;

        public Message(MessageType _type, string _value)
        {
            Type = _type;
            Value = _value;
        }

        public override string ToString()
        {
            return "Type: " + Type + " Param: " + Value;
        }
    }

    public static class Messages
    {
        public static byte[] Build(MessageType _type, string _message)
        {
            var typeData = BitConverter.GetBytes((uint) _type);
            var messageData = System.Text.Encoding.ASCII.GetBytes(_message);
            byte[] retMsg = new byte[4 + messageData.Length];
            typeData.CopyTo(retMsg, 0);
            messageData.CopyTo(retMsg, 4);
            return retMsg;
        }

        public static Message Parse(byte[] _bytes)
        {
            var type = (MessageType) BitConverter.ToInt32(_bytes.Take(4).ToArray(), 0);
            var param = _bytes.Skip(4).Take(_bytes.Length - 4).ToArray().String();
            return new Message(type, param);
        }
    }
}