#include <SPI.h>
#include "State.h"
#include "Rotex.h"

void State::resetAccumulators() {
  this->outPompaCaminoAccu_On=0;
  this->outPompaCaminoAccu_Off=0;
  this->outPompaAccu_On=0;
  this->outPompaAccu_Off=0;
  this->outCaldaiaAccu_On=0;
  this->outCaldaiaAccu_Off=0;
  this->outOverrideTermoAmbienteAccu_On = 0;
  this->outOverrideTermoAmbienteAccu_Off = 0;

  this->inTermoAmbienteAccu_On=0;
  this->inTermoAmbienteAccu_Off=0;
  this->inTermoAccumulatoreAccu_On=0;
  this->inTermoAccumulatoreAccu_Off=0;

  // rotexResetCounter = 0;
  this->rotexP1Accu_On = 0;
  this->lastAccuResetTime = millis();
}

void State::initVariables() {
  this->outPompaCaminoValue = LOW;
  this->outPompaValue = LOW;
  this->outCaldaiaValue = LOW;
  this->outOverrideTermoAmbienteValue = LOW;
  this->inTermoAmbienteValue=false;
  this->inTermoAccumulatoreValue=false;
  this->ainTempCaminoValue=0;
  this->ainTempCaminoValueCentigradi=0.0;
  this->lastTempAcquired = 0;
}

void State::statusToSerial ( boolean compactVersion ) {
  Serial.println( F("{") );

  Serial.print( F("  \"_type\": ") );
  if (!compactVersion) {
    Serial.print( F("\"accumulators\"") );
  }
  if (compactVersion) {
    Serial.print( F("\"data\"") );
  }
  Serial.println( F(",") );
  
  Serial.print( F("  \"loopStartMillis\": ") );
  Serial.print( this->loopStartMillis );
  Serial.println( F(",") );
  
  Serial.print( F("  \"outPompaValue\": ") );
  Serial.print( this->outPompaValue );
  Serial.println( F(",") );
  
  if (!compactVersion) {
    Serial.print( F("  \"outPompaAccu_On\": ") );
    Serial.print( this->outPompaAccu_On );
    Serial.println( F(",") );
  
    Serial.print( F("  \"outPompaAccu_Off\": ") );
    Serial.print( this->outPompaAccu_Off );
    Serial.println( F("," ));
  }
  
  Serial.print( F("  \"outPompaCaminoValue\": ") );
  Serial.print( this->outPompaCaminoValue );
  Serial.println( F(",") );
  
  if (!compactVersion) {
    Serial.print( F("  \"outPompaCaminoAccu_On\": ") );
    Serial.print( this->outPompaCaminoAccu_On );
    Serial.println( F(",") );
  
    Serial.print( F("  \"outPompaCaminoAccu_Off\": ") );
    Serial.print( this->outPompaCaminoAccu_Off );
    Serial.println( F(",") );
  }
  
  Serial.print( F("  \"outCaldaiaValue\": ") );
  Serial.print( this->outCaldaiaValue );
  Serial.println( F(",") );
  
  if (!compactVersion) {
    Serial.print( F("  \"outCaldaiaAccu_On\": ") );
    Serial.print( this->outCaldaiaAccu_On );
    Serial.println( F(",") );
  
    Serial.print( F("  \"outCaldaiaAccu_Off\": ") );
    Serial.print( this->outCaldaiaAccu_Off );
    Serial.println( F(",") );
  }  
  
  Serial.print( F("  \"inTermoAmbienteValue\": ") );
  Serial.print( this->inTermoAmbienteValue );
  Serial.println( F(",") );
  
  if (!compactVersion) {
    Serial.print( F("  \"inTermoAmbienteAccu_On\": ") );
    Serial.print( this->inTermoAmbienteAccu_On );
    Serial.println( F(",") );
  
    Serial.print( F("  \"inTermoAmbienteAccu_Off\": ") );
    Serial.print( this->inTermoAmbienteAccu_Off );
    Serial.println( F("," ));
  }  
  
  Serial.print( F("  \"inTermoAccumulatoreValue\": ") );
  Serial.print(  this->inTermoAccumulatoreValue);
  Serial.println( F(",") );

  if (!compactVersion) {
    Serial.print( F("  \"inTermoAccumulatoreAccu_On\": ") );
    Serial.print( this->inTermoAccumulatoreAccu_On );
    Serial.println( F(",") );
  
    Serial.print( F("  \"inTermoAccumulatoreAccu_Off\": ") );
    Serial.print( this->inTermoAccumulatoreAccu_Off );
    Serial.println( F(",") );
  }

  Serial.print( F("  \"outOverrideTermoAmbienteValue\": ") );
  Serial.print( this->outOverrideTermoAmbienteValue );
  Serial.println( F(",") );
 
  if (!compactVersion) {
    Serial.print( F("  \"isteLastOutCaldaia_On\": ") );
    Serial.print( this->isteLastOutCaldaia_On );
    Serial.println( F(",") );
  
    Serial.print( F("  \"isteLastOutCaldaia_On_For\": ") );
    Serial.print( this->isteLastOutCaldaia_On_For );
    Serial.println( F("," ));
        
    Serial.print( F("  \"rotexP1Accu_On\": ") );
    Serial.print( this->rotexP1Accu_On );
    Serial.println( F(",") );
  }
  
  Serial.print( F("  \"rotexHA\": " ));
  Serial.print( RotexStatus::rotexValues[ RTX_HA ] );
  Serial.println( F(",") );
  
  Serial.print( F("  \"rotexBK\": ") );
  Serial.print( RotexStatus::rotexValues[ RTX_BK ] );
  Serial.println( F(",") );
  
  Serial.print( F("  \"rotexP1\": " ));
  Serial.print( RotexStatus::rotexValues[ RTX_P1 ] );
  Serial.println( F(",") );
  
  Serial.print( F("  \"rotexP2\": ") );
  Serial.print( RotexStatus::rotexValues[ RTX_P2 ] );
  Serial.println( F("," ));
   
  Serial.print( F("  \"rotexTK\": ") );
  Serial.print( RotexStatus::rotexValues[ RTX_TK ] );
  Serial.println( F(",") );

  Serial.print( F("  \"rotexTR\": ") );
  Serial.print( RotexStatus::rotexValues[ RTX_TR ] );
  Serial.println( F(",") );

  Serial.print( F("  \"rotexTS\": ") );
  Serial.print( RotexStatus::rotexValues[ RTX_TS ] );
  Serial.println( F(",") );

  Serial.print( F("  \"rotexTV\": ") );
  Serial.print( RotexStatus::rotexValues[ RTX_TV ] );
  Serial.println( F(",") );

  Serial.print( F("  \"rotexPWR\": ") );
  Serial.print( RotexStatus::rotexValues[ RTX_PWR ] );
  Serial.println( F(",") );

  Serial.print( F("  \"rotexQT\": ") );
  Serial.print( RotexStatus::rotexValues[ RTX_QT ] );
  Serial.println( F(",") );



  Serial.print( F("  \"rotexPortataTV\": ") );
  Serial.print( RotexStatus::rotexPortataTV );
  Serial.println( F(",") );
  
  if (!compactVersion) {
    Serial.print( F("  \"rotexLastRead\": ") );
    Serial.print( RotexStatus::rotexLastRead );
    Serial.println( F(",") );
  }  
  Serial.print( F("  \"rotexLastReadString\": \"") );
  Serial.print( RotexStatus::rotexLastReadString );
  Serial.println( F("\",") );
  
  Serial.print( F("  \"ainTempCaminoValueCentigradi\": ") );
  Serial.print( this->ainTempCaminoValueCentigradi );

  Serial.println( F(",") );

  Serial.print( F("  \"ainTempCaminoValue\": ") );
  Serial.print( this->ainTempCaminoValue );
  Serial.println( F(",") );
  
  Serial.print( F("  \"rotexHasFailed\": ") );
  Serial.println( RotexStatus::rotexHasFailed );

  if (compactVersion) {
    Serial.println(F(""));
  }
  Serial.println( F("}") ); 
  
}
