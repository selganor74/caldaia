
using Infrastructure.Actions;

namespace CaldaiaBackend.Application.Commands 
{
    public class SendStringCommand : ICommand
    {
        public string ToSend { get; set; }
    }
}
