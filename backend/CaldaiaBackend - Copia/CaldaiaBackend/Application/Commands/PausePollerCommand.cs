using Infrastructure.Actions.Command;

namespace CaldaiaBackend.Application.Commands
{
    /// <summary>
    /// Mette in pausa il poller dei dati per 
    /// </summary>
    public class PausePollerCommand : ICommand
    {
        public string CommandId { get; }
        public int PauseForSeconds { get; set; }
    }
}