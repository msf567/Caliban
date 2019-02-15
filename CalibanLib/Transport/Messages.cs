using System;
using System.Linq;
using CalibanLib.Utility;

namespace CalibanLib.Transport
{
    public enum MessageType
    {
        GAME_CLOSE = 0x0000,
        WATERLEVEL_SET = 0x0101,
        WATERLEVEL_GET = 0x0102
    }

    public struct Message
    {
        public MessageType Type;
        public string Param;

        public Message(MessageType type, string param)
        {
            Type = type;
            Param = param;
        }

        public override string ToString()
        {
            return "Type: " + Type + " Param: " + Param;
        }
    }

    public static class Messages
    {
        public static byte[] Build(MessageType type, string message)
        {
            var typeData = BitConverter.GetBytes((uint) type);
            var messageData = System.Text.Encoding.ASCII.GetBytes(message);
            byte[] retMsg = new byte[4 + messageData.Length];
            typeData.CopyTo(retMsg, 0);
            messageData.CopyTo(retMsg, 4);
            return retMsg;
        }

        public static Message Parse(byte[] bytes)
        {
            var type = (MessageType) BitConverter.ToInt32(bytes.Take(4).ToArray(), 0);
            var param = bytes.Skip(4).Take(bytes.Length - 4).ToArray().String();
            return new Message(type, param);
        }
    }
}