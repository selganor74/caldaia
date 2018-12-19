#include <SPI.h>
#include <SoftwareSerial.h>

#include "SerialCommand.h"
#include "ntcTempLookup.h"
#include "HwSetup.h"
#include "Settings.h"
#include "Rotex.h"

// Numero di campioni memorizzati nel buffer circolare
#define  CB_VALUES  5

//..#define  CB_AVG_REBUILD_AFTER 1
//..#include <CircularBuffer.h>

// Contiene le impostazioni di runtime. (see Settings.h)
Settings runtimeSettings;

// Abilita o disabilita il debug tramite seriale
boolean serialDebug;

//.. CircularBuffer cbTempCaminoCentigradi = CircularBuffer();
//.. CircularBuffer cbRotexTS = CircularBuffer();
//.. CircularBuffer cbRotexPortataTV = CircularBuffer();

// In questa versione:
//   Aggiunto Override del termostato ambiente. Lo scopo è quello di mantenere la temperatura dell'accumulo tra i 60 ed i 65 gradi
//     quando il camino è in funzione. In questo modo, in caso di alte temperature del camino, c'è sempre sufficiente delta T per 
//     garantire un raffreddamento veloce. 
//     In estate, questo vincolo non deve esistere!
//   Aggiunta Gestione del Camino Vulcano. Lettura temperatura del camino ed azionamento pompa scambiatore Rotex.
//   Trasformato l'ingresso In-1 inTermoAccumulatore in un'uscita per comandare il reset (spegni e riaccendi del Rotex)
//   Rimosso l'ingresso "inTermoAccumulatore" chea usato per leggere lo stato del termostato infilato nel tubo di lettura del ROTEX.
//   Aggiunta lettura dei parametri del buzzo ROTEX
//   Aggiunta la possibilità di leggere temperature da trasduttori LM35 mediando i valori tramite un buffer circolare
//   Isteresi sull'output della caldaia. Se il termostato della caldaia
//   richiede calore, la caldaia rimane accesa per un tempo prestabilito
//   anziché fermarsi allo stop del termostato. Questo per evitare le
//   microaccensioni che attualmente si stanno verificando.
//   Reset Automatico della Ethernet tramite l'output 6 [pin]
//   Possibilità d interrogare arduino via ethernet
//   per avere lo stato dei sensori e degli attuatori.
//   Lo stato On/off dei digitali viene "Accumulato".
//   Ad ogni polling viene restituito il valore accumulato
//   di on e off. Se nella request compare la parola "resetAccu"
//   gli accumulatori vengono azzerati, dopo essere stati
//   trasmessi.
//
// In questa versione mancano:


// Oggetto SerialCommand per gestire i comandi da Seriale
SerialCommand SCmd = SerialCommand(); 

// Millisecondi di isteresi prima che la caldaia possa spegnersi 1h=60*60*1000=3600000  20min = 1200000
const unsigned long T_ISTERESI_CALDAIA = 1200000 ;
// Millisecondi tra un'acquisizione temperature e l'altra.
const unsigned long TEMP_SAMPLING_INTERVAL = 12000 ;
// Millis dell'ultima acquisizione temperature
unsigned long lastTempAcquired;
// Costanti per l'output
#define ACCESO "1"
#define SPENTO "0"

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


int   ainTempCaminoValue;
float ainTempCaminoValueCentigradi;



 
/************************
 *  FUNZIONI DI UTILITA *
 ************************/
// Calcola la durata di un intervallo, tenendo conto dell'eventuale ritorno a zero
// del contatore "millis()"
unsigned long calcolaIntervallo( unsigned long da, unsigned long a ) {
  return a >= da ? a - da : a + (0-1-da);
}



// Genera un impulso di lunghezza "milliseconds"
void pulse( unsigned long outputPin, unsigned long milliseconds ) {
  digitalWrite( outputPin, !bitRead( PORTD, outputPin ) );
  unsigned long startedAt = millis();
  while ( calcolaIntervallo( startedAt, millis() ) < milliseconds ) {
    // Attende che passino milliseconds millisecondi
  }
  digitalWrite( outputPin, !bitRead( PORTD, outputPin ) );
}

void blinkOutput( unsigned long outputId, unsigned long blinkInterval ) {
  static unsigned long last_millis[8] = {0,0,0,0,0,0,0,0};
  static boolean  last_values[8] = {false,false,false,false,false,false,false,false};

  if (calcolaIntervallo( last_millis[ outputId ], millis() ) > blinkInterval ) {
    last_values[ outputId ] = !last_values[ outputId ];
    last_millis[ outputId ] = millis();
    digitalWrite( outputId, !bitRead( PORTD, outputId ) );
  }
}

/***************************************
 * UTILITA PER IL PROGRAMMA PRINCIPALE *
 ***************************************/

void resetAccumulators() {
  outPompaCaminoAccu_On=0;
  outPompaCaminoAccu_Off=0;
  outPompaAccu_On=0;
  outPompaAccu_Off=0;
  outCaldaiaAccu_On=0;
  outCaldaiaAccu_Off=0;
  outOverrideTermoAmbienteAccu_On = 0;
  outOverrideTermoAmbienteAccu_Off = 0;

  inTermoAmbienteAccu_On=0;
  inTermoAmbienteAccu_Off=0;
  inTermoAccumulatoreAccu_On=0;
  inTermoAccumulatoreAccu_Off=0;

  // rotexResetCounter = 0;
  rotexP1Accu_On = 0;
  lastAccuResetTime = millis();
}

void initVariables() {

  outPompaCaminoValue = LOW;

  outPompaValue = LOW;

  outCaldaiaValue = LOW;

//  outRotexResetValue = HIGH;
//  rotexResetCounter = 0;

  outOverrideTermoAmbienteValue = LOW;

  inTermoAmbienteValue=false;

  inTermoAccumulatoreValue=false;

  ainTempCaminoValue=0;
  ainTempCaminoValueCentigradi=0.0;
  lastTempAcquired = 0;
}



/***************************************
 * DEFINIZIONE DEI COMANDI VIA SERIALE *
 ***************************************/

void statusToSerial ( boolean compactVersion ) {
  Serial.println( F("{") );
//*
  Serial.print( F("  \"loopStartMillis\": ") );
  Serial.print( loopStartMillis );
  Serial.println( F(",") );
  Serial.print( F("  \"outPompaValue\": ") );
  Serial.print( outPompaValue );
  Serial.println( F(",") );
  if (!compactVersion) {
  Serial.print( F("  \"outPompaAccu_On\": ") );
  Serial.print( outPompaAccu_On );
  Serial.println( F(",") );
  Serial.print( F("  \"outPompaAccu_Off\": ") );
  Serial.print( outPompaAccu_Off );
  Serial.println( F("," ));
  }
  Serial.print( F("  \"outPompaCaminoValue\": ") );
  Serial.print( outPompaCaminoValue );
  Serial.println( F(",") );
  if (!compactVersion) {
  Serial.print( F("  \"outPompaCaminoAccu_On\": ") );
  Serial.print( outPompaCaminoAccu_On );
  Serial.println( F(",") );
  Serial.print( F("  \"outPompaCaminoAccu_Off\": ") );
  Serial.print( outPompaCaminoAccu_Off );
  Serial.println( F(",") );
  }
  Serial.print( F("  \"outCaldaiaValue\": ") );
  Serial.print( outCaldaiaValue );
  Serial.println( F(",") );
  if (!compactVersion) {
  Serial.print( F("  \"outCaldaiaAccu_On\": ") );
  Serial.print( outCaldaiaAccu_On );
  Serial.println( F(",") );
  Serial.print( F("  \"outCaldaiaAccu_Off\": ") );
  Serial.print( outCaldaiaAccu_Off );
  Serial.println( F(",") );
  }  
  Serial.print( F("  \"inTermoAmbienteValue\": ") );
  Serial.print( inTermoAmbienteValue );
  Serial.println( F(",") );
  if (!compactVersion) {
  Serial.print( F("  \"inTermoAmbienteAccu_On\": ") );
  Serial.print( inTermoAmbienteAccu_On );
  Serial.println( F(",") );
  Serial.print( F("  \"inTermoAmbienteAccu_Off\": ") );
  Serial.print( inTermoAmbienteAccu_Off );
  Serial.println( F("," ));
  }  
  Serial.print( F("  \"inTermoAccumulatoreValue\": ") );
  Serial.print(  inTermoAccumulatoreValue);
  Serial.println( F(",") );
  if (!compactVersion) {
  Serial.print( F("  \"inTermoAccumulatoreAccu_On\": ") );
  Serial.print( inTermoAccumulatoreAccu_On );
  Serial.println( F(",") );
  Serial.print( F("  \"inTermoAccumulatoreAccu_Off\": ") );
  Serial.print( inTermoAccumulatoreAccu_Off );
  Serial.println( F(",") );
  
//  Serial.print( "  \"timeSinceLastAccuResetMs\": " );
//  Serial.print( timeSinceLastAccuResetMs );  
//  Serial.println( "," );
//  Serial.print( "  \"lastAccuResetTime\": " );
//  Serial.print( lastAccuResetTime );
//  Serial.println( "," );
//  
//  Serial.print( "  \"outRotexResetValue\": " );
//  Serial.print( outRotexResetValue );
//  Serial.println( "," );
  }
//  Serial.print( "  \"rotexResetCounter\": " );
//  Serial.print( rotexResetCounter );
//  Serial.println( "," );

  Serial.print( F("  \"outOverrideTermoAmbienteValue\": ") );
  Serial.print( outOverrideTermoAmbienteValue );
  Serial.println( F(",") );
 
  if (!compactVersion) {
  Serial.print( F("  \"isteLastOutCaldaia_On\": ") );
  Serial.print( isteLastOutCaldaia_On );
  Serial.println( F(",") );
  Serial.print( F("  \"isteLastOutCaldaia_On_For\": ") );
  Serial.print( isteLastOutCaldaia_On_For );
  Serial.println( F("," ));
//*/     
      
  Serial.print( F("  \"rotexP1Accu_On\": ") );
  Serial.print( rotexP1Accu_On );
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
  Serial.print( ainTempCaminoValueCentigradi );
/*
  if (!compactVersion) {
//*/
  Serial.println( F(",") );

  Serial.print( F("  \"ainTempCaminoValue\": ") );
  Serial.print( ainTempCaminoValue );
  Serial.println( F(",") );
  
  Serial.print( F("  \"rotexHasFailed\": ") );
  Serial.println( RotexStatus::rotexHasFailed );
/*
  }
//*/
  if (compactVersion) {
    Serial.println(F(""));
  }
  Serial.println( F("}") ); 
  
}

void cmdGetStatus() {
  statusToSerial( true );
}

void cmdGetStatusRA() {
  statusToSerial( false );
  resetAccumulators();
}

void cmdGetRuntimeSettings() {
  settingsToSerial(runtimeSettings);
}

void cmdLoadRuntimeSettings() {
  runtimeSettings = loadSettingsFromEEPROM();
  settingsToSerial( runtimeSettings );
}

void cmdSaveRuntimeSettings() {
  saveSettingsToEEPROM(runtimeSettings);
  cmdLoadRuntimeSettings();
}

void cmdRestoreSettingsToDefault() {
  runtimeSettings = getDefaultSettings();
  settingsToSerial( runtimeSettings );
}

void dummyMethod() {
  Serial.println(F("What?!"));
  return;
}

void serialCmdSetup () {


  // Setup dei comandi via Seriale
  SCmd.addCommand( "GET", cmdGetStatus ); // Restituisce lo stato delle variabili interne
  SCmd.addCommand( "GET-RA", cmdGetStatusRA ); // Restituisce lo stato delle variabili interne e resetta gli accumulatori
  SCmd.addCommand( "RESTORE", cmdRestoreSettingsToDefault ); 
  SCmd.addCommand( "GET-RS", cmdGetRuntimeSettings ); // Restituisce lo stato delle impostazioni
  SCmd.addCommand( "LOAD-RS", cmdLoadRuntimeSettings ); // Restituisce lo stato delle impostazioni
  SCmd.addCommand( "SAVE-RS", cmdSaveRuntimeSettings ); // Restituisce lo stato delle impostazioni
  SCmd.addCommand( "HELP", printHelp ); // Stampa un piccolo messaggio di Help
  SCmd.addDefaultHandler( dummyMethod ); // Handler for command that isn't matched (says "What?")
  
  Serial.println( F("Ready.") );  
}

void printHelp() {
  Serial.println(F("Available Commands:"));
  Serial.println(F("  GET      Obtains the status of the system."));
  Serial.println(F("  GET-RA   Same as GET plus some values and Resets all the Accumulatros variables."));
  Serial.println(F("  GET-RS   Returns a json with the runtime settings."));
  Serial.println(F("  LOAD-RS  Loads settings from the EEPROM. Will revert to defaults if not valid."));
  Serial.println(F("  SAVE-RS  Saves the runtime Settings."));
  Serial.println(F("  RESTORE  Restores the default Settings."));
  Serial.println(F("  HELP     Prints this help screen."));
};

void ioSetup() {
  pinMode( outPompaCamino, OUTPUT );
  pinMode( outPompa, OUTPUT );
  pinMode( outCaldaia, OUTPUT );
  pinMode( inTermoAmbiente, INPUT );
  // pinMode( outRotexReset, OUTPUT );
  pinMode( outOverrideTermoAmbiente, OUTPUT );
  pinMode( inTermoAccumulatore, INPUT );
  // pinMode( ainTempCamino, INPUT );
}


// Lettura dei valori
void doReadInputs() {
  inTermoAmbienteValue = digitalRead( inTermoAmbiente );
  // Serial.print( "inTermoAmbiente=" );
  // Serial.println( inTermoAmbienteValue == HIGH ? ACCESO : SPENTO );

  inTermoAccumulatoreValue = digitalRead( inTermoAccumulatore );
  // Serial.print( "inTermoAccumulatore=" );
  // Serial.println( inTermoAccumulatoreValue == HIGH ? ACCESO : SPENTO );
  
  // La temperatura viene calcolata acquisendo "CB_VALUES" valori, eliminando il massimo ed il minimo ed facendo la media dei restanti.
  static int currentCaminoTemp = 0;
  static int Temps[CB_VALUES];
  static char tempIndex= 0;
  static char maxIndex = 0;
  static char minIndex = 0;
  static int maxTemp = 0;
  static int minTemp = 1023;
  int valuesToAverage = 0;
  static float lastValidAinTempCaminoValueCentigradi = 0.0;
  if (  calcolaIntervallo( lastTempAcquired, millis() ) >= TEMP_SAMPLING_INTERVAL ) {
    lastTempAcquired = millis();
//    ainTempCaminoValue = analogRead( ainTempCamino );
//    ainTempCaminoValueCentigradi = (float)getTempFromAin( ainTempCaminoValue );
    if ( tempIndex >= CB_VALUES ) {
      tempIndex = 0;
      ainTempCaminoValue = 0;
      valuesToAverage = 0;
      for ( char i = 0; i < CB_VALUES; i++ ) {
        if ( i != minIndex && i != maxIndex ) {
          ainTempCaminoValue += Temps[ i ];
          valuesToAverage ++;
        }
      } 
      
      ainTempCaminoValue /= valuesToAverage;
      ainTempCaminoValueCentigradi = (float)getTempFromAin(ainTempCaminoValue);
      // 2016 10 12 - Compensazione linearizzazione sonda NTC. Dalle ultime misurazioni pare che la lookup legga
      // almeno 4 gradi in più rispetto alla realtà. Il risultato è che la pompa parte quando il camino è ancora troppo freddo
      // causando condensa.
      // 2017 01 12 - Visto l'incendio della canna fumaria, riduciamo l'offset a 2 gradi
      ainTempCaminoValueCentigradi = ainTempCaminoValueCentigradi > 2 ? ainTempCaminoValueCentigradi - 2 : 0 ;
      
      // 2017 01 16 - Per evitare i valori a zero che ogni tanto capitano, faccio in modo che se la lettura analogica 
      //              è zero, prendo l'ultima lettura valida. 
      if (ainTempCaminoValueCentigradi == 0) {
        ainTempCaminoValueCentigradi = lastValidAinTempCaminoValueCentigradi;
      } else {
        lastValidAinTempCaminoValueCentigradi = ainTempCaminoValueCentigradi;
      }
      // infine resetta i valori minimo e massimo e l'indice corrente per il prossimo giro
      tempIndex = 0;
      maxIndex = 0;
      minIndex = 0;
      maxTemp = 0;
      minTemp = 1023;      
    }
    if ( tempIndex < CB_VALUES ) {
      //ainTempCaminoValue = analogRead( ainTempCamino );
      // a quanto pare la misura del corto circuito in non restituisce il desiderato 1023 ma 
      // restituisce 420. Per avere il range massimo si è dovuto moltiplicare il valore letto
      // 1023 / 420 = 2,4357 a cui aggiungere un 0.5 prima del "troncamento"
      // currentCaminoTemp = (int)( ( (float)analogRead( ainTempCamino ) * 1.1493  ) + 0.5 );
      currentCaminoTemp = (int)( ( (float)analogRead( ainTempCamino ) ) + 50);
      //currentCaminoTemp = (int)( ( (float)analogRead( ainTempCamino ) ) );
      Temps[ tempIndex ] = currentCaminoTemp;
      if ( currentCaminoTemp > maxTemp ) {
        maxTemp = currentCaminoTemp;
        maxIndex = tempIndex;
      }
      if ( currentCaminoTemp <= minTemp ) {
        minTemp = currentCaminoTemp;
        minIndex = tempIndex;
      }
      tempIndex++;
    }
  }
}

// Elaborazione dei valori letti
void doCrunchInputs() {
  // Elaborazione degli output
  //* Gestione Caldaia tramite cbRotexTS usato come termostato
  char sogliaMin;
  char sogliaMax;

//*
  if ( RotexStatus::rotexValues[ RTX_TS ] != 0 ) {
    sogliaMin = runtimeSettings.rotexTermoMin; 
    sogliaMax = runtimeSettings.rotexTermoMax; 
    if( outCaldaiaValue == LOW ? RotexStatus::rotexValues[ RTX_TS ] < sogliaMin : RotexStatus::rotexValues[ RTX_TS ] > sogliaMax ) {
      outCaldaiaValue == HIGH ? outCaldaiaValue = LOW : outCaldaiaValue = HIGH;
    }
  }  else {

    // Gestione dell'accesnione tramite termostato + isteresi temporale
    if ( inTermoAccumulatoreValue == HIGH ) {
      if ( outCaldaiaValue == LOW ) {
        outCaldaiaValue = HIGH;
        isteLastOutCaldaia_On = millis();
        isteLastOutCaldaia_On_For = 0;
      }
    }
    if ( inTermoAccumulatoreValue == LOW ) {
      if ( outCaldaiaValue == HIGH ) {
        isteLastOutCaldaia_On_For = calcolaIntervallo( isteLastOutCaldaia_On, millis() );
        if ( isteLastOutCaldaia_On_For > T_ISTERESI_CALDAIA ) {
          outCaldaiaValue = LOW;
          isteLastOutCaldaia_On = 0;
          isteLastOutCaldaia_On_For = 0;
        }
      }
    }
  }
  
  // Gestione del termocamino
  // Facciamo partire la pompa del camino in base a delle fasce:
  // se tCamino < 55 allora la pompa parte solo se la differenza di temperatura tra TS e TCamino >= 5° e TS <= 44
  // se tCamino >= 55 allora la pompa parte solo se la differenza è >= 5°
  // la pompa deve rimanere accesa almeno 30 secondi consecutivi 
  static unsigned long tOnPompaCamino = 0;
  static unsigned long tOffPompaCamino = 0;
#define TEMPO_IN_OFF ( calcolaIntervallo( tOffPompaCamino, millis() ) )
#define TEMPO_IN_ON ( calcolaIntervallo( tOnPompaCamino, millis() ) )
#define TEMPO_MINIMO_PER_STATO 60000 
#define PUO_CAMBIARE_STATO ( outPompaCaminoValue == LOW ? TEMPO_IN_OFF >= TEMPO_MINIMO_PER_STATO : TEMPO_IN_ON >= TEMPO_MINIMO_PER_STATO )
#define ROTEX_DISPONIBILE ( RotexStatus::rotexValues[ RTX_TS ] != 0 )
#define ROTEX_NON_E_DISPONIBILE !ROTEX_DISPONIBILE
#define DELTA_CAMINO_ROTEX ( (int)ainTempCaminoValueCentigradi - (int)RotexStatus::rotexValues[ RTX_TS ] )
#define T_CAMINO (int)ainTempCaminoValueCentigradi
#define ACCENDI_POMPA_CAMINO      tOnPompaCamino  = millis(); outPompaCaminoValue = HIGH;          
#define SPEGNI_POMPA_CAMINO       tOffPompaCamino = millis(); outPompaCaminoValue = LOW;          
#define POMPA_CAMINO_E_SPENTA     outPompaCaminoValue == LOW
#define DELTA_CAMINO_ROTEX_SOPRA_SOGLIA_INNESCO     DELTA_CAMINO_ROTEX >= runtimeSettings.deltaTInnescoPompaCamino  /* Ad esempio 3 gradi */
#define DELTA_CAMINO_ROTEX_SOTTO_SOGLIA_INNESCO     DELTA_CAMINO_ROTEX < runtimeSettings.deltaTInnescoPompaCamino  /* Ad esempio 3 gradi */
#define T_CAMINO_SOPRA_SOGLIA_INNESCO               T_CAMINO > runtimeSettings.TCaminoPerAccensionePompa            /* Ad esempio 62 gradi */
  //*
  if ( PUO_CAMBIARE_STATO ) {
    if (  POMPA_CAMINO_E_SPENTA ) {
      if ( ROTEX_DISPONIBILE ) {
        if ( DELTA_CAMINO_ROTEX_SOPRA_SOGLIA_INNESCO || T_CAMINO_SOPRA_SOGLIA_INNESCO ) {
          ACCENDI_POMPA_CAMINO
        }
      }
      if ( !ROTEX_NON_E_DISPONIBILE ) {
        if ( T_CAMINO >= runtimeSettings.TInnescoSeRotexNonDisponibile /* Ad esempio 55 */ ) {
          ACCENDI_POMPA_CAMINO
        }  
      } 
    } else {   // if ( outPompaCaminoValue == LOW )
      if ( ROTEX_DISPONIBILE ) {
        if ( DELTA_CAMINO_ROTEX_SOTTO_SOGLIA_INNESCO ) {
          SPEGNI_POMPA_CAMINO
        }
      }
      if ( !ROTEX_DISPONIBILE ) {
        if ( T_CAMINO < runtimeSettings.TDisinnescoSeRotexNonDisponibile /* Ad esempio 52 */ ) {
          SPEGNI_POMPA_CAMINO
        }         
      }     
    }          // if ( outPompaCaminoValue == LOW )
  }          // if ( PUO_CAMBIARE_STATO )
  //*/
  /*
  if ( PUO_CAMBIARE_STATO ) {
    if ( outPompaCaminoValue == LOW ) { // La pompa è spenta vediamo se possiamo accenderla
      if (RotexStatus::rotexValues[ RTX_TS ] != 0 ) {
        if ((int)ainTempCaminoValueCentigradi - (int)RotexStatus::rotexValues[ RTX_TS ] >= 5 ) {
          if ( ainTempCaminoValueCentigradi <= 53.0 ) {
            if ( RotexStatus::rotexValues[ RTX_TS ] < 45 ) {
              tOnPompaCamino = millis();
              outPompaCaminoValue = HIGH;
            }
          } else {
              tOnPompaCamino = millis();
              outPompaCaminoValue = HIGH;          
          }
        }
      } else {
        // non è disponibile una lettura rotex
        if ( ainTempCaminoValueCentigradi >= 53.0 ) {
              tOnPompaCamino = millis();
              outPompaCaminoValue = HIGH;           
        }
      } 
    } else { // La pompa è accesa vediamo se possiamo spegnerla
      // outPompaCaminoValue
      if (RotexStatus::rotexValues[ RTX_TS ] != 0 ) {
        if ( ainTempCaminoValueCentigradi <= 53.0 ) {
          if ( ( RotexStatus::rotexValues[ RTX_TS ] >= 45 ) || ( (int)ainTempCaminoValueCentigradi - (int)RotexStatus::rotexValues[ RTX_TS ] < 5 ) ) {
            tOffPompaCamino = millis();
            outPompaCaminoValue = LOW;
          }
        } else {
          if (((int)ainTempCaminoValueCentigradi - (int)RotexStatus::rotexValues[ RTX_TS ] < 3 ) && ( ainTempCaminoValueCentigradi <= 59.0 ) ) {
            tOffPompaCamino = millis();
            outPompaCaminoValue = LOW;          
          }
        }
      } else {
        // Non è disponbibile una lettura ROTEX
        if ( ainTempCaminoValueCentigradi <= 59.0 ) {
            tOffPompaCamino = millis();
            outPompaCaminoValue = LOW; 
        }
      }
    }
  } // if PUO_CAMBIARE_STATO 
  //*/
  /* La pompa del riscaldamento è direttamente collegata alla richiesta di calore nel da parte dei termostati ambiente.*/
  inTermoAmbienteValue     == HIGH ? outPompaValue   = HIGH : outPompaValue = LOW;
}

void doManageOverrideTermostatoAmbiente() {
#define SE_OVERRIDE_TERMOSTATO_AMBIENTE_NON_E_ATTIVO    outOverrideTermoAmbienteValue == LOW

  if (ainTempCaminoValueCentigradi == 0.0) {
    return;
  }
    
  if (ROTEX_NON_E_DISPONIBILE) {
    if ( SE_OVERRIDE_TERMOSTATO_AMBIENTE_NON_E_ATTIVO ) {
      if (ainTempCaminoValueCentigradi > runtimeSettings.TInnescoOverrideTermostatoSeRotexNonDisponibile /* Ad Esempio 65 */ ) {
        outOverrideTermoAmbienteValue = HIGH;    
      }
    } else {
      if (ainTempCaminoValueCentigradi <= runtimeSettings.TInnescoOverrideTermostatoSeRotexNonDisponibile  /* Ad Esempio 60 */) {
        outOverrideTermoAmbienteValue = LOW;    
      }  
    }    
    return;
  }

  if (ainTempCaminoValueCentigradi <= runtimeSettings.TDisinnescoOverrideSeRotexDisponibile /* Ad esempio 60 */) {
    outOverrideTermoAmbienteValue = LOW;
    return;
  }

  // Se la temperatura è maggiore di 60, supponiamo che il camino stia funzionando
  if ( outOverrideTermoAmbienteValue == LOW ) {
    if (RotexStatus::rotexValues[RTX_TS] >= runtimeSettings.rotexMaxTempConCamino) {
      outOverrideTermoAmbienteValue = HIGH;    
    }
  } else {
    if (RotexStatus::rotexValues[RTX_TS] <= runtimeSettings.rotexMinTempConCamino) {
      outOverrideTermoAmbienteValue = LOW;    
    }  
  }
}

void doManageAccumulators() {
  unsigned long Accu_ActualTS;
  unsigned long Accu_Length;
  static unsigned long Accu_LastTS = 0;

  if (Accu_LastTS == 0 ) Accu_LastTS = millis();

  Accu_ActualTS = millis();
  Accu_Length = calcolaIntervallo(Accu_LastTS, Accu_ActualTS); // Accu_ActualTS >= Accu_LastTS ? Accu_ActualTS - Accu_LastTS : Accu_ActualTS + (0-1-Accu_LastTS);

  outPompaCaminoValue      == HIGH ? outPompaCaminoAccu_On      += Accu_Length : outPompaCaminoAccu_Off      += Accu_Length;
  inTermoAmbienteValue     == HIGH ? inTermoAmbienteAccu_On     += Accu_Length : inTermoAmbienteAccu_Off     += Accu_Length;
  inTermoAccumulatoreValue == HIGH ? inTermoAccumulatoreAccu_On += Accu_Length : inTermoAccumulatoreAccu_Off += Accu_Length;
  outPompaValue            == HIGH ? outPompaAccu_On            += Accu_Length : outPompaAccu_Off            += Accu_Length;
  outCaldaiaValue          == HIGH ? outCaldaiaAccu_On          += Accu_Length : outCaldaiaAccu_Off          += Accu_Length;
  
  outOverrideTermoAmbienteValue == HIGH ? outOverrideTermoAmbienteAccu_On += Accu_Length : outOverrideTermoAmbienteAccu_Off += Accu_Length;
  
  Accu_LastTS = Accu_ActualTS;
  rotexP1Accu_On += RotexStatus::rotexValues[ RTX_P1 ] != 0 ?  Accu_Length : 0;
//  timeSinceLastAccuResetMs = calcolaIntervallo( lastAccuResetTime, Accu_ActualTS );
  timeSinceLastAccuResetMs = inTermoAmbienteAccu_On + inTermoAmbienteAccu_Off;

}

// Impostazione dell'output
void doSetOutputs() {
  digitalWrite( outPompaCamino,           outPompaCaminoValue );   
  digitalWrite( outPompa,                 outPompaValue   );
  digitalWrite( outCaldaia,               outCaldaiaValue );
  digitalWrite( outOverrideTermoAmbiente, outOverrideTermoAmbienteValue );
  // La logica di reset è contenuta nella funzione;
  // doRotexReset();
}


/***********************
 *   METODI PRINCIPALI *
 ***********************/

void setup() {
    // Utilizziamo la seriale per fare un po' di debug.
  Serial.begin(9600);
  serialDebug = false;
  
  runtimeSettings = loadSettingsFromEEPROM();
  ioSetup();
  
  serialCmdSetup();
  rotexSerialSetup();
  
  initVariables();

  resetAccumulators();

  // imposto il riferimeno analogico agli 1.1 volt interni
  // analogReference( INTERNAL );
  // Imposta il reference 5V
  analogReference( DEFAULT );

}

void loop() {
  loopStartMillis = millis();
  // lettura delle variabili.
  doReadInputs();
  // Processa i comandi in arrivo dalla seriale
  rotexReadSerial();
  // elaborazione degli ingressi
  doCrunchInputs();
  // Gestione dell'override del termostato
  doManageOverrideTermostatoAmbiente();
  // impostazione degli output
  doSetOutputs();
  // gestione degli accumulatori
  doManageAccumulators();

  if(calcolaIntervallo(lastEmittedValues, loopStartMillis) >= 5000) {
    lastEmittedValues = loopStartMillis;
    // statusToSerial(false);
  }
  // 
  SCmd.readSerial();
}
