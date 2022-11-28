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
    public Temperature? ROTEX_TEMP_ACCUMULO { get; internal set; }
}
#pragma warning restore CS8618
