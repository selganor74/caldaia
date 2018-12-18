#include <EEPROM.h>
#include <SPI.h>

#include "Settings.h"

void settingsToSerial() {
  Serial.println( F("{") );
  Serial.print( F("  \"rotexTermoMin\": ") );
  Serial.print( int(currentSettings.rotexTermoMin) );
  Serial.println( F(",") );
  Serial.print( F("  \"rotexTermoMax\": ") );
  Serial.print( int(currentSettings.rotexTermoMax) );
  Serial.println( F(",") );
  Serial.print( F("  \"deltaSolare\": ") );
  Serial.print( int(currentSettings.deltaSolare) );
  Serial.println( F(",") );
  Serial.print( F("  \"rotexMaxTempConCamino\": ") );
  Serial.print( int(currentSettings.rotexMaxTempConCamino) );
  Serial.println( F(",") );
  Serial.print( F("  \"rotexMinTempConCamino\": ") );
  Serial.print( int(currentSettings.rotexMinTempConCamino) );
  Serial.println( F(",") );
  Serial.print( F("  \"T_ISTERESI_CALDAIA\": ") );
  Serial.print( currentSettings.T_ISTERESI_CALDAIA );
  Serial.println( F(",") );
  Serial.print( F("  \"TEMP_SAMPLING_INTERVAL\": ") );
  Serial.print( currentSettings.TEMP_SAMPLING_INTERVAL );
  Serial.println();
  Serial.print( F("}") );
}

bool headerIsValid(char *header) {
  return header[0] == 'C' && header[1] == 'S' && header[2] == 'V' && header[3] == '1';
}

bool loadSettings() {
  bool toReturn = false;
  Settings loadedSettings;
  EEPROM.get(0, loadedSettings);
  if(headerIsValid(loadedSettings.header)) {
    currentSettings.rotexTermoMin          = loadedSettings.rotexTermoMin;
    currentSettings.rotexTermoMax          = loadedSettings.rotexTermoMax ;      
    currentSettings.deltaSolare            = loadedSettings.deltaSolare;       
    currentSettings.rotexMaxTempConCamino  = loadedSettings.rotexMaxTempConCamino;                                    
    currentSettings.rotexMinTempConCamino  = loadedSettings.rotexMinTempConCamino; 
    currentSettings.T_ISTERESI_CALDAIA     = loadedSettings.T_ISTERESI_CALDAIA;
    currentSettings.TEMP_SAMPLING_INTERVAL = loadedSettings.TEMP_SAMPLING_INTERVAL;

    Serial.println(F("Trovate impostazioni valide."));
    toReturn = true;
  } else {
    Serial.println(F("Impostazioni non valide. Default attivi"));
  }
  settingsToSerial();
  return toReturn;
}

void saveSettings() {
  EEPROM.put(0, currentSettings);  
}
