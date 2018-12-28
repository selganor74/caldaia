using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Actions.Command;

namespace CaldaiaBackend.Application.Commands
{
    /// <summary>
    /// Reset The arduino Board by flashing the DTR line on the (virtual) serial port.
    /// </summary>
    public class ResetArduinoCommand : ICommand
    {
        public string CommandId { get; } = Guid.NewGuid().ToString();
    }
}
