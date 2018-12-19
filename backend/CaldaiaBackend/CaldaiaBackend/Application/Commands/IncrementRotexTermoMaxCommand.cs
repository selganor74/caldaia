using Infrastructure.Actions.Command;

namespace CaldaiaBackend.Application.Commands
{
    /// <summary>
    /// Incrementa il valore della variabile rotexTermoMax
    /// Se la temperatura dell'accumulo ROTEX sale oltre questa soglia
    /// la caldaia si spegne.
    /// </summary>
    public class IncrementRotexTermoMaxCommand : ICommand
    {
        public string CommandId { get; }
    }
}
