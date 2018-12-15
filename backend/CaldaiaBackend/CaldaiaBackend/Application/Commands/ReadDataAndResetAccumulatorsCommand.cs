using System;
using Infrastructure.Actions.Command;

namespace CaldaiaBackend.Application.Commands
{
    public class ReadDataAndResetAccumulatorsCommand : ICommand
    {
        public string CommandId { get; } = Guid.NewGuid().ToString();
    }
}
