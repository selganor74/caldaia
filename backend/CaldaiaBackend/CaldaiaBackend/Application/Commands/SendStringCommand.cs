using Infrastructure.Actions.Command;

namespace CaldaiaBackend.Application.Commands 
{
    public class SendStringCommand : ICommand
    {
        public string CommandId { get; }

        public string ToSend { get; set; }
    }
}
