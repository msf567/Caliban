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
        BIOME_SWITCH = 0x0006,
        CHOREO = 0x0007,
        GRAPHICS_SCENE = 0x0008,
        HEARTBEAT = 0x0100,
        WATERLEVEL_SET = 0x0101,
        WATERLEVEL_GET = 0x0102,
        WATERLEVEL_ADD = 0x0103,
        MAP_REVEAL = 0x0105,
        TEST_MESSAGE = 0x0106,
        HOOKS_L_CLICK = 0x10AAA,

        SANDSTORM_START = 0x010A
    }

    public struct Message
    {
        public readonly string SenderID;
        public readonly MessageType Type;
        public readonly string Value;

        public Message(string senderId, MessageType _type, string _value)
        {
            SenderID = senderId;
            Type = _type;
            Value = _value;
        }

        public override string ToString()
        {
            return $"SenderID: {SenderID} Type: {Type} Param: {Value}";
        }
    }

    public static class Messages
    {
        public static byte[] Build(string senderId, MessageType type, byte[] message)
        {
            senderId ??= string.Empty;
            message ??= Array.Empty<byte>();

            byte[] typeBytes = BitConverter.GetBytes((uint)type);
            byte[] senderBytes = Encoding.UTF8.GetBytes(senderId);
            byte[] senderLengthBytes = BitConverter.GetBytes(senderBytes.Length);

            byte[] result = new byte[4 + 4 + senderBytes.Length + message.Length];

            // Wire format: [4-byte Type][4-byte SenderID Length][SenderID UTF-8 Bytes][Payload Bytes]
            Buffer.BlockCopy(typeBytes, 0, result, 0, 4);
            Buffer.BlockCopy(senderLengthBytes, 0, result, 4, 4);
            Buffer.BlockCopy(senderBytes, 0, result, 8, senderBytes.Length);
            Buffer.BlockCopy(message, 0, result, 8 + senderBytes.Length, message.Length);

            return result;
        }

        public static byte[] Build(string senderId, MessageType type, string message)
        {
            byte[] messageData = Encoding.UTF8.GetBytes(message ?? string.Empty);
            return Build(senderId, type, messageData);
        }

        public static Message Parse(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 8)
            {
                throw new ArgumentException("Invalid message packet: payload is too small.", nameof(bytes));
            }

            // Extract Type (bytes 0..3)
            var type = (MessageType)BitConverter.ToUInt32(bytes, 0);

            // Extract SenderID Length (bytes 4..7)
            int senderLength = BitConverter.ToInt32(bytes, 4);
            if (senderLength < 0 || 8 + senderLength > bytes.Length)
            {
                throw new ArgumentException("Invalid sender length specified in header.", nameof(bytes));
            }

            // Extract SenderID (bytes 8..8 + senderLength)
            string senderId = Encoding.UTF8.GetString(bytes, 8, senderLength);

            // Extract Payload (remaining bytes)
            int payloadOffset = 8 + senderLength;
            int payloadLength = bytes.Length - payloadOffset;
            string payload = Encoding.UTF8.GetString(bytes, payloadOffset, payloadLength);

            return new Message(senderId, type, payload);
        }
    }
}