using System;
using System.Linq;
using System.Text;

namespace Caliban.Core.Transport
{
    public enum MessageType 
    {
        APP_CLOSE = 0x0000,
        GAME_CLOSE = 0x1000,
        GAME_START = 0x9999,
        DEBUG_LOG = 0x0001,
        REGISTER = 0x0002,
        
        GAME_WIN = 0x0003,
        GAME_LOSE = 0x0004,
        CONSUME_TREASURE = 0x0005,
        ZONE_SWITCH = 0x0006,
        
        WATERLEVEL_SET = 0x0101,
        WATERLEVEL_GET = 0x0102,
        WATERLEVEL_ADD = 0x0103,
        MAP_REVEAL = 0x0105,
        
        HOOKS_L_CLICK = 0x10AAA,
        
        SANDSTORM_START = 0x010A
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
            var messageData = Encoding.ASCII.GetBytes(_message);
            byte[] retMsg = new byte[4 + messageData.Length];
            typeData.CopyTo(retMsg, 0);
            messageData.CopyTo(retMsg, 4);
            return retMsg;
        }

        public static Message Parse(byte[] _bytes)
        {
            var type = (MessageType) BitConverter.ToInt32(_bytes.Take(4).ToArray(), 0);
            var param = String( _bytes.Skip(4).Take(_bytes.Length - 4).ToArray());
            return new Message(type, param);
        }
        
        private static string String(byte[] _bytes)
        {
            var chars = new char[_bytes.Length];
            var d = Encoding.UTF8.GetDecoder();
            d.GetChars(_bytes, 0, _bytes.Length, chars, 0);
            var szData = new string(chars);

            return szData;
        }
    }
}