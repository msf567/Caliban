using Caliban.Core.Transport;

namespace Hooks
{
    public class HooksClient : ClientApp
    {
        public HooksClient(string _clientName, bool _shouldRegister = true) : base(_clientName, _shouldRegister)
        {
            
        }

        public void OnLeftClick()
        {
            SendMessageToHost(Messages.Build(MessageType.HOOKS_L_CLICK, ""));
        }
    }
}