using System.Text;
using domain.measures;

namespace application;
#pragma warning disable CS8618
public class CaldaiaAllValues
{
    // Comanda la pompa di ricircolo tra CAMINO e ACCUMULO ROTEX
    public OnOff STATO_RELAY_POMPA_CAMINO { get; set; }

    // Permette di attivare i riscaldamenti dell'appartamento, per abbassare la temperatura dell'accumulo
    public OnOff STATO_RELAY_BYPASS_TERMOSTATO_AMBIENTE { get; set; }

    // Attiva la pompa del riscaldamento a pavimento, quando i termostati ambiente lo richiedono.
    public OnOff STATO_RELAY_POMPA_RISCALDAMENTO { get; set; }

    // Comanda l'accensione della caldaia.
    public OnOff STATO_RELAY_CALDAIA { get; set; }

    // Legge il valore de
    public OnOff TERMOSTATO_AMBIENTI { get; set; }
    public OnOff TERMOSTATO_ROTEX { get; set; }
    public Temperature TEMPERATURA_CAMINO { get; set; }

    // Misura derivata da un comparatore con isteresi applicato a TEMPERATURA_CAMINO
    public OnOff CAMINO_ON_OFF { get; set; }
    public Temperature ROTEX_TEMP_ACCUMULO { get; internal set; }
    public Temperature ROTEX_TEMP_PANNELLI { get; internal set; }
    public OnOff ROTEX_STATO_POMPA { get; internal set; }


    public override string ToString()
    {
        var sb = $@"
OUTPUTS:
    {nameof(STATO_RELAY_BYPASS_TERMOSTATO_AMBIENTE)}: {STATO_RELAY_BYPASS_TERMOSTATO_AMBIENTE?.FormattedValue ?? "null"}
    {nameof(STATO_RELAY_CALDAIA)}: {STATO_RELAY_CALDAIA?.FormattedValue ?? "null"}
    {nameof(STATO_RELAY_POMPA_CAMINO)}: {STATO_RELAY_POMPA_CAMINO?.FormattedValue ?? "null"}
    {nameof(STATO_RELAY_POMPA_RISCALDAMENTO)}: {STATO_RELAY_POMPA_RISCALDAMENTO?.FormattedValue ?? "null"}
    
INPUTS: 
    {nameof(this.TEMPERATURA_CAMINO)}: {this.TEMPERATURA_CAMINO?.FormattedValue ?? "null"}
    {nameof(this.CAMINO_ON_OFF)}: {this.CAMINO_ON_OFF?.FormattedValue ?? "null"}
    {nameof(this.ROTEX_TEMP_ACCUMULO)}: {this.ROTEX_TEMP_ACCUMULO?.FormattedValue ?? "null"}
    {nameof(this.TERMOSTATO_ROTEX)}: {this.TERMOSTATO_ROTEX?.FormattedValue ?? "null"}
    {nameof(this.TERMOSTATO_AMBIENTI)}: {this.TERMOSTATO_AMBIENTI?.FormattedValue ?? "null"}

READINGS:
    {nameof(this.ROTEX_TEMP_PANNELLI)}: {this.ROTEX_TEMP_PANNELLI?.FormattedValue ?? "null"}
    {nameof(this.ROTEX_STATO_POMPA)}: {this.ROTEX_STATO_POMPA?.FormattedValue ?? "null"}
";
        return sb;
    }
}
#pragma warning restore CS8618
