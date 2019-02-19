using System.Net.Sockets;

namespace Caliban.Core.Transport
{
    public delegate void TcpTerminalMessageRecivedDel(Socket _socket, byte[] message);

    public delegate void TcpTerminalConnectDel(Socket _socket);

    public delegate void TcpTerminalDisconnectDel(Socket _socket);
}