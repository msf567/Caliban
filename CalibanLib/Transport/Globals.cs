using System.Net.Sockets;

namespace CalibanLib.Transport
{
    public delegate void TcpTerminalMessageRecivedDel(Socket socket, byte[] message);
    public delegate void TcpTerminalConnectDel(Socket socket);
    public delegate void TcpTerminalDisconnectDel(Socket socket);
}
