using System.Threading;
using Caliban.Core.Transport;

namespace TornMap
{
    public class MapPiece : ClientApp
    {
        public MapPiece(string _clientName) : base(_clientName, false)
        {
            int timeout = 10;
            while (!IsConnected && timeout > 0)
            {
                timeout--;
                Thread.Sleep(10);
            }
            
            SendMessageToHost(Messages.Build(MessageType.MAP_REVEAL, ""));

            KillSelf("TornMap.exe");
            Deconstruct();
        }
    }
}