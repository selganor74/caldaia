
using Infrastructure.Actions;

namespace CaldaiaBackend.Application.Commands
{
    /// <summary>
    /// Decrementa il valore della variabile rotexTermoMax
    /// Se la temperatura dell'accumulo ROTEX sale oltre questa soglia
    /// la caldaia si spegne.
    /// </summary>
    public class DecrementRotexTermoMaxCommand : ICommand
    {
        public string CommandId { get; }
    }
}
