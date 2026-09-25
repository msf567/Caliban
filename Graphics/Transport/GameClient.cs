using System.Net.Sockets;
using Caliban.Core.Transport;

namespace Caliban.Graphics.Transport;

internal sealed class AppTransportClient(App app) : ClientApp("Graphics")
{
    private readonly App _app = app;

    protected override void ClientOnConnected(Socket _socket)
    {
        base.ClientOnConnected(_socket);
        _app.TransportClient = this;
    }

    protected override void ClientOnMessageReceived(byte[] _message)
    {
        _app.ClientOnMessageReceived(_message);
    }
}