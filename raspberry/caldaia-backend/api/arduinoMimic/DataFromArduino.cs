using application;

namespace api.arduinoMimic;

public class DataFromArduino
{
    public static DataFromArduino? From(CaldaiaAllValues stato) 
    {
        var toReturn = new DataFromArduino();

        toReturn.outPompaValue = (int)stato.STATO_RELAY_POMPA_RISCALDAMENTO.DigitalValue;
        toReturn.outPompaCaminoValue = (int)stato.STATO_RELAY_POMPA_CAMINO.DigitalValue;
        toReturn.outCaldaiaValue = (int)stato.STATO_RELAY_CALDAIA.DigitalValue;
        toReturn.outOverrideTermoAmbienteValue = (int)stato.STATO_RELAY_BYPASS_TERMOSTATO_AMBIENTE.DigitalValue;
        toReturn.inTermoAmbienteValue = (int)stato.TERMOSTATO_AMBIENTI.DigitalValue;
        toReturn.ainTempCaminoValueCentigradi = (float)stato.TEMPERATURA_CAMINO.Value;
        toReturn.rotexTK = (int)stato.ROTEX_TEMP_PANNELLI.Value;
        toReturn.rotexTS = (int)stato.ROTEX_TEMP_ACCUMULO.Value;
        toReturn.rotexP1 = (int)stato.ROTEX_STATO_POMPA.DigitalValue;

        return toReturn;
    }

    public string timestamp { get; set; } = DateTime.UtcNow.ToString("o");
    public string _type { get; set; } = "data"; // can also be "accumulators"
    public ulong loopStartMillis { get; set; }
    public int outPompaValue { get; set; }
    public ulong outPompaAccu_On { get; set; }
    public ulong outPompaAccu_Off { get; set; }
    public int outPompaCaminoValue { get; set; }
    public ulong outPompaCaminoAccu_On { get; set; }
    public ulong outPompaCaminoAccu_Off { get; set; }
    public int outCaldaiaValue { get; set; }
    public ulong outCaldaiaAccu_On { get; set; }
    public ulong outCaldaiaAccu_Off { get; set; }
    public int inTermoAmbienteValue { get; set; }
    public ulong inTermoAmbienteAccu_On { get; set; }
    public ulong inTermoAmbienteAccu_Off { get; set; }
    public int inTermoAccumulatoreValue { get; set; }
    public ulong inTermoAccumulatoreAccu_On { get; set; }
    public ulong inTermoAccumulatoreAccu_Off { get; set; }
    public int outOverrideTermoAmbienteValue { get; set; }
    public ulong isteLastOutCaldaia_On { get; set; }
    public ulong isteLastOutCaldaia_On_For { get; set; }
    public ulong rotexP1Accu_On { get; set; }
    public int rotexHA { get; set; }
    public int rotexBK { get; set; }
    public int rotexP1 { get; set; }
    public int rotexP2 { get; set; }
    public int rotexTK { get; set; }
    public int rotexTR { get; set; }
    public int rotexTS { get; set; }
    public int rotexTV { get; set; }
    public int rotexPWR { get; set; }
    public int rotexQT { get; set; }
    public float rotexPortataTV { get; set; }
    public int rotexLastRead { get; set; }
    public string rotexLastReadString { get; set; }
    public float ainTempCaminoValueCentigradi { get; set; }
    public int ainTempCaminoValue { get; set; }
    public int rotexHasFailed { get; set; }
}
