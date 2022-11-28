using domain.systemComponents;
using domain.measures;
using application.subSystems;
using application.infrastructure;

namespace application;

public class CaldaiaIOSet : IDisposable
{
    // Tutti i sotto sistemi
    public Rotex ROTEX { get; set; }
    public Camino CAMINO { get; set; }
    public CaldaiaMetano CALDAIA { get; set; }
    public Riscaldamento RISCALDAMENTO { get; set; }

    // Variabili per l'accesso di tutti i singoli dispositivi
    public DigitalOutput RELAY_POMPA_CAMINO => CAMINO.RELAY_POMPA_CAMINO;
    public DigitalOutput RELAY_BYPASS_TERMOSTATO_AMBIENTE => RISCALDAMENTO.RELAY_BYPASS_TERMOSTATO_AMBIENTE;
    public DigitalOutput RELAY_POMPA_RISCALDAMENTO => RISCALDAMENTO.RELAY_POMPA_RISCALDAMENTO;
    public AnalogInput<Temperature> CAMINO_TEMPERATURA => CAMINO.CAMINO_TEMPERATURA;
    public DigitalInput CAMINO_ON_OFF => CAMINO.CAMINO_ON_OFF;

    public AnalogInput<Temperature> ROTEX_TEMP_ACCUMULO => ROTEX.ROTEX_TEMP_ACCUMULO;
    public AnalogInput<Temperature> ROTEX_TEMP_PANNELLI => ROTEX.ROTEX_TEMP_PANNELLI;
    public DigitalInput ROTEX_STATO_POMPA => ROTEX.ROTEX_STATO_POMPA;
    public DigitalOutput RELAY_CALDAIA => CALDAIA.RELAY_ACCENSIONE_CALDAIA;

    public DigitalInput TERMOSTATO_AMBIENTI => RISCALDAMENTO.TERMOSTATO_AMBIENTI;
    public DigitalInput TERMOSTATO_ROTEX => ROTEX.TERMOSTATO_ROTEX;

    public CaldaiaIOSet(
        Rotex rOTEX,
        Camino cAMINO,
        CaldaiaMetano cALDAIA,
        Riscaldamento rISCALDAMENTO)
    {
        ROTEX = rOTEX;
        CAMINO = cAMINO;
        CALDAIA = cALDAIA;
        RISCALDAMENTO = rISCALDAMENTO;
    }

    // true when all "real" inputs have a value
    public bool IsReady()
    {
        return !(RELAY_POMPA_CAMINO.LastMeasure == null
            || RELAY_BYPASS_TERMOSTATO_AMBIENTE.LastMeasure == null
            || RELAY_POMPA_RISCALDAMENTO.LastMeasure == null
            || CAMINO_ON_OFF.LastMeasure == null
            || CAMINO_TEMPERATURA.LastMeasure == null
            || RELAY_CALDAIA.LastMeasure == null
            || TERMOSTATO_AMBIENTI.LastMeasure == null
            || TERMOSTATO_ROTEX.LastMeasure == null
            || ROTEX_TEMP_ACCUMULO.LastMeasure == null
        );
    }

    public CaldaiaAllValues ReadAll()
    {
        while (!IsReady())
            Thread.Sleep(100);

        var toReturn = new CaldaiaAllValues();

#pragma warning disable CS8601
        toReturn.STATO_RELAY_BYPASS_TERMOSTATO_AMBIENTE = RELAY_BYPASS_TERMOSTATO_AMBIENTE.LastMeasure;
        toReturn.STATO_RELAY_POMPA_CAMINO = RELAY_POMPA_CAMINO.LastMeasure;
        toReturn.STATO_RELAY_CALDAIA = RELAY_CALDAIA.LastMeasure;
        toReturn.STATO_RELAY_POMPA_RISCALDAMENTO = RELAY_POMPA_RISCALDAMENTO.LastMeasure;

        toReturn.TERMOSTATO_AMBIENTI = TERMOSTATO_AMBIENTI.LastMeasure;
        toReturn.TERMOSTATO_ROTEX = TERMOSTATO_ROTEX.LastMeasure;
        toReturn.ROTEX_TEMP_ACCUMULO = ROTEX_TEMP_ACCUMULO.LastMeasure;

        toReturn.TEMPERATURA_CAMINO = CAMINO_TEMPERATURA.LastMeasure ?? new Temperature(0);
        toReturn.CAMINO_ON_OFF = CAMINO_ON_OFF.LastMeasure ?? new OnOff(OnOffState.OFF);
#pragma warning restore CS8601

        return toReturn;
    }

    // Disposes every property that can be disposed
    public void Dispose()
    {
        this.DisposeDisposables();
    }
}
#pragma warning restore CS8618
