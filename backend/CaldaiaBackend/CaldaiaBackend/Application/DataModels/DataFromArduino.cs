using System;

namespace CaldaiaBackend.Application.DataModels
{
    public class DataFromArduino
    {
        public string timestamp { get; set; } = DateTime.UtcNow.ToString("o");
        public int loopStartMillis { get; set; }
        public int outPompaValue { get; set; }
        public int outPompaAccu_On { get; set; }
        public int outPompaAccu_Off { get; set; }
        public int outPompaCaminoValue { get; set; }
        public int outPompaCaminoAccu_On { get; set; }
        public int outPompaCaminoAccu_Off { get; set; }
        public int outCaldaiaValue { get; set; }
        public int outCaldaiaAccu_On { get; set; }
        public int outCaldaiaAccu_Off { get; set; }
        public int inTermoAmbienteValue { get; set; }
        public int inTermoAmbienteAccu_On { get; set; }
        public int inTermoAmbienteAccu_Off { get; set; }
        public int inTermoAccumulatoreValue { get; set; }
        public int inTermoAccumulatoreAccu_On { get; set; }
        public int inTermoAccumulatoreAccu_Off { get; set; }
        public int outOverrideTermoAmbienteValue { get; set; }
        public int isteLastOutCaldaia_On { get; set; }
        public int isteLastOutCaldaia_On_For { get; set; }
        public int rotexP1Accu_On { get; set; }
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
}
/*
{
  "loopStartMillis": 2850000,
  "outPompaValue": 0,
  "outPompaAccu_On": 0,
  "outPompaAccu_Off": 2849999,
  "outPompaCaminoValue": 0,
  "outPompaCaminoAccu_On": 0,
  "outPompaCaminoAccu_Off": 2849999,
  "outCaldaiaValue": 0,
  "outCaldaiaAccu_On": 0,
  "outCaldaiaAccu_Off": 2849999,
  "inTermoAmbienteValue": 0,
  "inTermoAmbienteAccu_On": 0,
  "inTermoAmbienteAccu_Off": 2849999,
  "inTermoAccumulatoreValue": 0,
  "inTermoAccumulatoreAccu_On": 0,
  "inTermoAccumulatoreAccu_Off": 2849999,
  "outOverrideTermoAmbienteValue": 0,
  "isteLastOutCaldaia_On": 0,
  "isteLastOutCaldaia_On_For": 0,
  "rotexP1Accu_On": 0,
  "rotexHA": 0,
  "rotexBK": 0,
  "rotexP1": 0,
  "rotexP2": 0,
  "rotexTK": 0,
  "rotexTR": 0,
  "rotexTS": 0,
  "rotexTV": 0,
  "rotexPWR": 0,
  "rotexQT": 0,
  "rotexPortataTV": 0.00,
  "rotexLastRead": 0,
  "rotexLastReadString": "",
  "ainTempCaminoValueCentigradi": 12.00,
  "ainTempCaminoValue": 309,
  "rotexHasFailed": 0 
}
*/
