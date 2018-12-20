#include "Settings.h"

#include <EEPROM.h>
#include <SPI.h>

void settingsToSerial(Settings settings) {
  Serial.println( F("{") );
  
  Serial.print( F("  \"_type\": ") );
  Serial.print( F("\"settings\"") );
  Serial.println( F(",") );

  Serial.print( F("  \"rotexTermoMin\": ") );
  Serial.print( int(settings.rotexTermoMin) );
  Serial.println( F(",") );
  Serial.print( F("  \"rotexTermoMax\": ") );
  Serial.print( int(settings.rotexTermoMax) );
  Serial.println( F(",") );
  Serial.print( F("  \"rotexMaxTempConCamino\": ") );
  Serial.print( int(settings.rotexMaxTempConCamino) );
  Serial.println( F(",") );
  Serial.print( F("  \"rotexMinTempConCamino\": ") );
  Serial.print( int(settings.rotexMinTempConCamino) );
  Serial.println( F(",") );
  Serial.print( F("  \"T_ISTERESI_CALDAIA\": ") );
  Serial.print( settings.T_ISTERESI_CALDAIA );
  Serial.println( F(",") );
  Serial.print( F("  \"TEMP_SAMPLING_INTERVAL\": ") );
  Serial.print( settings.TEMP_SAMPLING_INTERVAL );
  Serial.println( F(",") );
  Serial.print( F("  \"deltaTInnescoPompaCamino\": ") );
  Serial.print( int(settings.deltaTInnescoPompaCamino) );
  Serial.println( F(",") );
  Serial.print( F("  \"TCaminoPerAccensionePompa\": ") );
  Serial.print( int(settings.TCaminoPerAccensionePompa) );
  Serial.println( F(",") );
  Serial.print( F("  \"TInnescoSeRotexNonDisponibile\": ") );
  Serial.print( int(settings.TInnescoSeRotexNonDisponibile) );
  Serial.println( F(",") );
  Serial.print( F("  \"TDisinnescoSeRotexNonDisponibile\": ") );
  Serial.print( int(settings.TDisinnescoSeRotexNonDisponibile) );
  Serial.println( F(",") );
  Serial.print( F("  \"TInnescoOverrideTermostatoSeRotexNonDisponibile\": ") );
  Serial.print( int(settings.TInnescoOverrideTermostatoSeRotexNonDisponibile) );
  Serial.println( F(",") );
  Serial.print( F("  \"TDisinnescoOverrideTermostatoSeRotexNonDisponibile\": ") );
  Serial.print( int(settings.TDisinnescoOverrideTermostatoSeRotexNonDisponibile) );
  Serial.println( F(",") );
  Serial.print( F("  \"TDisinnescoOverrideSeRotexDisponibile\": ") );
  Serial.print( int(settings.TDisinnescoOverrideSeRotexDisponibile) );
  Serial.println();
  Serial.print( F("}") );
}


bool headerIsValid(char *header) {
  return  header[0] == HEADER[0]/*'C'*/ && 
          header[1] == HEADER[1]/*'S'*/ && 
          header[2] == HEADER[2]/*'V'*/ && 
          header[3] == HEADER[3]/*'3'*/;
}

Settings getDefaultSettings() {
  static Settings defaultSettings;
  return defaultSettings;
}

Settings loadSettingsFromEEPROM() {
  static Settings loadedSettings;
  EEPROM.get(0, loadedSettings);
  if(headerIsValid(loadedSettings.header)) {
    Serial.println(F("Trovate impostazioni valide."));
  } else {
    Serial.println(F("Impostazioni non valide. Default attivi"));
    loadedSettings = getDefaultSettings();
  }
  return loadedSettings;
}

void saveSettingsToEEPROM(Settings settings) {
  EEPROM.put(0, settings);  
}
