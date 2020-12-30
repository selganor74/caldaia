﻿using Infrastructure.Actions;

namespace CaldaiaBackend.Application.Commands
{
    /// <summary>
    /// Decrementa il valore della variabile rotexTermoMin
    /// Se la temperatura dell'accumulo ROTEX scende sotto questa soglia
    /// la caldaia parte.
    /// </summary>
    public class DecrementRotexTermoMinCommand : ICommand
    {
    }
}
