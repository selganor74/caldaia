using Infrastructure.Actions.Command;

namespace CaldaiaBackend.Application.Commands
{
    /// <summary>
    /// Incrementa il valore della variabile rotexTermoMin
    /// Se la temperatura dell'accumulo ROTEX scende sotto questa soglia
    /// la caldaia parte.
    /// </summary>
    public class IncrementRotexTermoMinCommand : ICommand
    {
        public string CommandId { get; }
    }
}
