using domain.systemComponents;
using domain.measures;
using application.subSystems;
using application.infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics;

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
    public AnalogInput CAMINO_TEMPERATURA => CAMINO.CAMINO_TEMPERATURA;
    public DigitalInput CAMINO_ON_OFF => CAMINO.CAMINO_ON_OFF;

    public AnalogInput ROTEX_TEMP_ACCUMULO => ROTEX.ROTEX_TEMP_ACCUMULO;
    public AnalogInput ROTEX_TEMP_PANNELLI => ROTEX.ROTEX_TEMP_PANNELLI;
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

    private static DateTime LastPrinted = DateTime.Now;

    // true when all "real" inputs have a value
    public bool IsReady(ILogger? log)
    {
        if (log == null)
            log = NullLoggerFactory.Instance.CreateLogger(nameof(CaldaiaIOSet));

        var timeout = Stopwatch.StartNew();
        var TIMEOUT = TimeSpan.FromSeconds(15);
        var isReady = () => ROTEX.IsReady()
            && CAMINO.IsReady()
            && CALDAIA.IsReady()
            && RISCALDAMENTO.IsReady();

        while (!isReady() && timeout.Elapsed < TIMEOUT)
        {
            Thread.Sleep(100);
        }
        if (!isReady())
        {
            if (!ROTEX.IsReady()) ROTEX.SetAsNotReady();
            if (!CAMINO.IsReady()) CAMINO.SetAsNotReady();
            if (!CALDAIA.IsReady()) CALDAIA.SetAsNotReady();
            if (!RISCALDAMENTO.IsReady()) RISCALDAMENTO.SetAsNotReady();
            
            return false;
        }

        return true;
    }

    public CaldaiaAllValues ReadAll(ILogger? log)
    {
        //var READY_TIMEOUT = TimeSpan.FromSeconds(1);
        //while (!IsReady(log))
        //{
        //    Thread.Sleep(100);
        //}

        var toReturn = new CaldaiaAllValues();

#pragma warning disable CS8600
#pragma warning disable CS8601
        toReturn.STATO_RELAY_BYPASS_TERMOSTATO_AMBIENTE = (OnOff)RELAY_BYPASS_TERMOSTATO_AMBIENTE.LastMeasure;
        toReturn.STATO_RELAY_POMPA_CAMINO = (OnOff)RELAY_POMPA_CAMINO.LastMeasure;
        toReturn.STATO_RELAY_CALDAIA = (OnOff)RELAY_CALDAIA.LastMeasure;
        toReturn.STATO_RELAY_POMPA_RISCALDAMENTO = (OnOff)RELAY_POMPA_RISCALDAMENTO.LastMeasure;

        toReturn.TERMOSTATO_AMBIENTI = (OnOff)TERMOSTATO_AMBIENTI.LastMeasure;
        toReturn.TERMOSTATO_ROTEX = (OnOff)TERMOSTATO_ROTEX.LastMeasure;
        toReturn.ROTEX_TEMP_ACCUMULO = (Temperature)ROTEX_TEMP_ACCUMULO.LastMeasure;
        toReturn.ROTEX_TEMP_PANNELLI = (Temperature)ROTEX_TEMP_PANNELLI.LastMeasure;

        toReturn.TEMPERATURA_CAMINO = (Temperature)CAMINO_TEMPERATURA.LastMeasure ?? new Temperature(0);
        toReturn.CAMINO_ON_OFF = (OnOff)CAMINO_ON_OFF.LastMeasure ?? new OnOff(OnOffState.OFF);
        toReturn.ROTEX_STATO_POMPA = (OnOff)ROTEX_STATO_POMPA.LastMeasure ?? new OnOff(OnOffState.OFF);
#pragma warning restore CS8601
#pragma warning restore CS8600

        return toReturn;
    }

    

    // Disposes every property that can be disposed
    public void Dispose()
    {
        this.DisposeDisposables(null);
    }
}
#pragma warning restore CS8618
