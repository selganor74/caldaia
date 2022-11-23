using domain.systemComponents;
using domain.measures;
using domain.meters;

namespace application;

#pragma warning disable CS8618
public class CaldaiaAllValues
{
    public OnOffState RELAY_POMPA_CAMINO { get; set; }
    public OnOffState RELAY_BYPASS_TERMOSTATO_AMBIENTE { get; set; }
    public OnOffState RELAY_POMPA_RISCALDAMENTO { get; set; }
    public OnOffState RELAY_CALDAIA { get; set; }
    public OnOffState TERMOSTATO_AMBIENTI { get; set; }
    public OnOffState TERMOSTATO_ROTEX { get; set; }
}

public class CaldaiaIOSet : IDisposable
{
    public DigitalOutput RELAY_POMPA_CAMINO { get; set; }
    public DigitalIOMeter CaminoStatoPompaMeter { get; set; }

    public DigitalOutput RELAY_BYPASS_TERMOSTATO_AMBIENTE { get; set; }
    public DigitalOutput RELAY_POMPA_RISCALDAMENTO { get; set; }
    public AnalogInput<Temperature> CaminoTemp { get; set; }

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
        AnalogInput<Temperature> caminoTemp,
        AnalogInput<Temperature> rotexTempAccumulo,
        AnalogInput<Temperature> rotexTempPannelli,
        DigitalInput rotexStatoPompa
        )
    {
        RELAY_POMPA_CAMINO = rELAY_POMPA_CAMINO;
        CaminoStatoPompaMeter = new DigitalIOMeter(RELAY_POMPA_CAMINO);
        RELAY_BYPASS_TERMOSTATO_AMBIENTE = rELAY_BYPASS_TERMOSTATO_AMBIENTE;
        RELAY_POMPA_RISCALDAMENTO = rELAY_POMPA_RISCALDAMENTO;
        CaminoTemp = caminoTemp;
        RotexTempAccumulo = rotexTempAccumulo;
        RotexTempPannelli = rotexTempPannelli;
        RotexStatoPompa = rotexStatoPompa;
        RotexStatoPompaMeter = new DigitalIOMeter(RotexStatoPompa);
        RELAY_CALDAIA = rELAY_CALDAIA;
        CaldaiaStatoAccensioneMeter = new DigitalIOMeter(RELAY_CALDAIA);
        TERMOSTATO_AMBIENTI = tERMOSTATO_AMBIENTI;
        TERMOSTATO_ROTEX = tERMOSTATO_ROTEX;
    }

    public CaldaiaAllValues ReadAll()
    {
        var toReturn = new CaldaiaAllValues();

#pragma warning disable CS8602
        toReturn.RELAY_BYPASS_TERMOSTATO_AMBIENTE = RELAY_BYPASS_TERMOSTATO_AMBIENTE.DigitalValue;
        toReturn.RELAY_POMPA_CAMINO = RELAY_POMPA_CAMINO.DigitalValue;
        toReturn.RELAY_CALDAIA = RELAY_CALDAIA.DigitalValue;
        toReturn.RELAY_POMPA_RISCALDAMENTO = RELAY_POMPA_RISCALDAMENTO.DigitalValue;

        toReturn.TERMOSTATO_AMBIENTI = TERMOSTATO_AMBIENTI.DigitalValue;
        toReturn.TERMOSTATO_ROTEX = TERMOSTATO_ROTEX.DigitalValue;
#pragma warning restore CS8602

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
