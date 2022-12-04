namespace api.arduinoMimic;

public class DataFromArduino
{
    public DateTime timestamp { get; set; }
    public decimal loopStartMillis { get; set; }
    public decimal outPompaValue { get; set; }
    public decimal outPompaAccu_On { get; set; }
    public decimal outPompaAccu_Off { get; set; }
    public decimal outPompaCaminoValue { get; set; }
    public decimal outPompaCaminoAccu_On { get; set; }
    public decimal outPompaCaminoAccu_Off { get; set; }
    public decimal outCaldaiaValue { get; set; }
    public decimal outCaldaiaAccu_On { get; set; }
    public decimal outCaldaiaAccu_Off { get; set; }
    public decimal inTermoAmbienteValue { get; set; }
    public decimal inTermoAmbienteAccu_On { get; set; }
    public decimal inTermoAmbienteAccu_Off { get; set; }
    public decimal inTermoAccumulatoreValue { get; set; }
    public decimal inTermoAccumulatoreAccu_On { get; set; }
    public decimal inTermoAccumulatoreAccu_Off { get; set; }
    public decimal outOverrideTermoAmbienteValue { get; set; }
    public decimal isteLastOutCaldaia_On { get; set; }
    public decimal isteLastOutCaldaia_On_For { get; set; }
    public decimal rotexP1Accu_On { get; set; }
    public decimal rotexHA { get; set; }
    public decimal rotexBK { get; set; }
    public decimal rotexP1 { get; set; }
    public decimal rotexP2 { get; set; }
    public decimal rotexTK { get; set; }
    public decimal rotexTR { get; set; }
    public decimal rotexTS { get; set; }
    public decimal rotexTV { get; set; }
    public decimal rotexPWR { get; set; }
    public decimal rotexQT { get; set; }
    public decimal rotexPortataTV { get; set; }
    public decimal rotexLastRead { get; set; }
    public string rotexLastReadString { get; set; }
    public decimal ainTempCaminoValueCentigradi { get; set; }
    public decimal ainTempCaminoValue { get; set; }
    public decimal rotexHasFailed { get; set; }

}
