using domain.systemComponents;
using domain.measures;
using domain.measures.meters;

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
    public Temperature TEMPERATURA_ROTEX { get; set; }
    public Temperature TEMPERATURA_CAMINO { get; set; }
    
    // Misura derivata da un comparatore con isteresi applicato a TEMPERATURA_CAMINO
    public OnOff CAMINO_ON_OFF { get; set; }
}

public class CaldaiaIOSet : IDisposable
{
    public DigitalOutput RELAY_POMPA_CAMINO { get; set; }
    public DigitalIOMeter CaminoStatoPompaMeter { get; set; }

    public DigitalOutput RELAY_BYPASS_TERMOSTATO_AMBIENTE { get; set; }
    public DigitalOutput RELAY_POMPA_RISCALDAMENTO { get; set; }
    public AnalogInput<Temperature> CAMINO_TEMPERATURA { get; set; }
    public DigitalInput CAMINO_ON_OFF { get; set; }

    public AnalogInput<Temperature> RotexTempAccumulo { get; set; }

    public AnalogInput<Temperature> RotexTempPannelli { get; set; }

    public DigitalInput RotexStatoPompa { get; set; }
    public DigitalIOMeter RotexStatoPompaMeter { get; set; }

    public DigitalOutput RELAY_CALDAIA { get; set; }
    public DigitalIOMeter CaldaiaStatoAccensioneMeter { get; set; }

    public DigitalInput TERMOSTATO_AMBIENTI { get; set; }
    public DigitalInput TERMOSTATO_ROTEX { get; set; }

    public CaldaiaIOSet(
        DigitalOutput rELAY_POMPA_CAMINO,
        DigitalOutput rELAY_BYPASS_TERMOSTATO_AMBIENTE,
        DigitalOutput rELAY_POMPA_RISCALDAMENTO,
        DigitalOutput rELAY_CALDAIA,
        DigitalInput tERMOSTATO_AMBIENTI,
        DigitalInput tERMOSTATO_ROTEX,
        AnalogInput<Temperature> caminoTemperatura,
        AnalogInput<Temperature> rotexTempAccumulo,
        AnalogInput<Temperature> rotexTempPannelli,
        DigitalInput rotexStatoPompa,
        DigitalInput cAMINO_ON_OFF
        )
    {
        RELAY_POMPA_CAMINO = rELAY_POMPA_CAMINO;
        CaminoStatoPompaMeter = new DigitalIOMeter(RELAY_POMPA_CAMINO);
        RELAY_BYPASS_TERMOSTATO_AMBIENTE = rELAY_BYPASS_TERMOSTATO_AMBIENTE;
        RELAY_POMPA_RISCALDAMENTO = rELAY_POMPA_RISCALDAMENTO;
        CAMINO_TEMPERATURA = caminoTemperatura;
        RotexTempAccumulo = rotexTempAccumulo;
        RotexTempPannelli = rotexTempPannelli;
        RotexStatoPompa = rotexStatoPompa;
        RotexStatoPompaMeter = new DigitalIOMeter(RotexStatoPompa);
        RELAY_CALDAIA = rELAY_CALDAIA;
        CaldaiaStatoAccensioneMeter = new DigitalIOMeter(RELAY_CALDAIA);
        TERMOSTATO_AMBIENTI = tERMOSTATO_AMBIENTI;
        TERMOSTATO_ROTEX = tERMOSTATO_ROTEX;
        CAMINO_ON_OFF = cAMINO_ON_OFF;
    }

    public CaldaiaAllValues ReadAll()
    {
        var toReturn = new CaldaiaAllValues();

#pragma warning disable CS8601
        toReturn.STATO_RELAY_BYPASS_TERMOSTATO_AMBIENTE = RELAY_BYPASS_TERMOSTATO_AMBIENTE.LastMeasure;
        toReturn.STATO_RELAY_POMPA_CAMINO = RELAY_POMPA_CAMINO.LastMeasure;
        toReturn.STATO_RELAY_CALDAIA = RELAY_CALDAIA.LastMeasure;
        toReturn.STATO_RELAY_POMPA_RISCALDAMENTO = RELAY_POMPA_RISCALDAMENTO.LastMeasure;

        toReturn.TERMOSTATO_AMBIENTI = TERMOSTATO_AMBIENTI.LastMeasure;
        toReturn.TERMOSTATO_ROTEX = TERMOSTATO_ROTEX.LastMeasure;

        toReturn.TEMPERATURA_CAMINO = CAMINO_TEMPERATURA.LastMeasure ?? new Temperature(0);
        toReturn.CAMINO_ON_OFF = CAMINO_ON_OFF.LastMeasure ?? new OnOff(OnOffState.OFF);
#pragma warning restore CS8601

        return toReturn;
    }

    // Disposes every property that can be disposed
    public void Dispose()
    {
        var allProps = GetType().GetProperties();
        var allDisposables = allProps.Where(p => typeof(IDisposable).IsAssignableFrom(p.PropertyType)).ToList();
        foreach (var disposableProperty in allDisposables)
        {
            var disposable = disposableProperty.GetValue(this) as IDisposable;
            disposable?.Dispose();
        }
    }
}
#pragma warning restore CS8618
