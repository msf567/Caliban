using System.Net.Sockets;
using Caliban.Core.Transport;

namespace Caliban.Graphics.Transport;

internal sealed class AppTransportClient(OverlayWindow window) : ClientApp("Graphics")
{
    private readonly OverlayWindow _window = window;

    protected override void ClientOnConnected(Socket _socket)
    {
        base.ClientOnConnected(_socket);
        _window.TransportClient = this;
    }

    protected override void ClientOnMessageReceived(byte[] _message)
    {
        _window.OnMessageReceived(_message);
    }
}
