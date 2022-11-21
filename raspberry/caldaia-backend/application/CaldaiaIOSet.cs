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

public class CaldaiaIOSet
{
    public DigitalOutput RELAY_POMPA_CAMINO { get; set; }
    public DigitalInputMeter CaminoStatoPompaMeter { get; set; }

    public DigitalOutput RELAY_BYPASS_TERMOSTATO_AMBIENTE { get; set; }
    public DigitalOutput RELAY_POMPA_RISCALDAMENTO { get; set; }
    public AnalogInput<Temperature> CaminoTemp { get; set; }

    public AnalogInput<Temperature> RotexTempAccumulo { get; set; }

    public AnalogInput<Temperature> RotexTempPannelli { get; set; }

    public DigitalInput RotexStatoPompa { get; set; }
    public DigitalInputMeter RotexStatoPompaMeter { get; set; }

    public DigitalOutput RELAY_CALDAIA { get; set; }
    public DigitalInputMeter CaldaiaStatoAccensioneMeter { get; set; }

    public DigitalInput TERMOSTATO_AMBIENTI { get; set; }
    public DigitalInput TERMOSTATO_ROTEX { get; set; }

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
}
#pragma warning restore CS8618
