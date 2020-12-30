using Infrastructure.Actions;

namespace CaldaiaBackend.Application.Commands
{
    /// <summary>
    /// Mette in pausa il poller dei dati per 
    /// </summary>
    public class PausePollerCommand : ICommand
    {
        public int PauseForSeconds { get; set; }
    }
}