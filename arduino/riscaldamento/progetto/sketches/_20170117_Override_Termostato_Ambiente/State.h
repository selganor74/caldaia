#ifndef STATE_H
#define STATE_H

// Costanti per l'output
#define ACCESO "1"
#define SPENTO "0"

class State {
  public:
  
  void initVariables();
  void resetAccumulators();
  void statusToSerial( boolean compactVersion );
  
  // Millisecondi di isteresi prima che la caldaia possa spegnersi 1h=60*60*1000=3600000  20min = 1200000
  const unsigned long T_ISTERESI_CALDAIA = 1200000 ;
  // Millisecondi tra un'acquisizione temperature e l'altra.
  const unsigned long TEMP_SAMPLING_INTERVAL = 12000 ;
  // Millis dell'ultima acquisizione temperature
  unsigned long lastTempAcquired;
  
  unsigned long loopStartMillis;
  unsigned int  loopLengthMillis;
  
  boolean       outPompaCaminoValue;
  unsigned long outPompaCaminoAccu_On;
  unsigned long outPompaCaminoAccu_Off;
  
  boolean       outPompaValue;
  unsigned long outPompaAccu_On;
  unsigned long outPompaAccu_Off;
  
  boolean       outCaldaiaValue;
  unsigned long outCaldaiaAccu_On;
  unsigned long outCaldaiaAccu_Off;
  
  boolean       inTermoAmbienteValue;
  unsigned long inTermoAmbienteAccu_On;
  unsigned long inTermoAmbienteAccu_Off;
  
  boolean       inTermoAccumulatoreValue;
  unsigned long inTermoAccumulatoreAccu_On;
  unsigned long inTermoAccumulatoreAccu_Off;
  
  unsigned long timeSinceLastAccuResetMs;
  unsigned long lastAccuResetTime;
  
  unsigned long lastEmittedValues;
  // boolean outRotexResetValue;
  // unsigned long rotexResetCounter;
  
  unsigned long rotexP1Accu_On;
  
  unsigned long isteLastOutCaldaia_On;
  unsigned long isteLastOutCaldaia_On_For;
  
  boolean       outOverrideTermoAmbienteValue;
  unsigned long outOverrideTermoAmbienteAccu_On;
  unsigned long outOverrideTermoAmbienteAccu_Off;
  
  long  ainTempCaminoValue;
  float ainTempCaminoValueCentigradi;
};


void resetAccumulators(State toReset);

void initVariables(State toInit);

#endif
