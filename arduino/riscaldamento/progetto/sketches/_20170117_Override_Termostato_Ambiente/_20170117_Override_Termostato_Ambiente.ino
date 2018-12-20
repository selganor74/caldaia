#include <SPI.h>
#include <SoftwareSerial.h>

#include "SerialCommand.h"
#include "ntcTempLookup.h"
#include "HwSetup.h"
#include "Settings.h"
#include "Rotex.h"
#include "State.h"
#include "Utils.h"

// Numero di campioni memorizzati nel buffer circolare
#define  CB_VALUES  5

#define MIN_ROTEX_TERMO_MIN 35
#define MAX_ROTEX_TERMO_MAX 65

// Contiene le impostazioni di runtime. (see Settings.h)
static Settings runtimeSettings;

// Contiene lo stato "attuale" del sistema 
static State currentState;

// Oggetto SerialCommand per gestire i comandi da Seriale
static SerialCommand SCmd = SerialCommand(); 



/***************************************
 * DEFINIZIONE DEI COMANDI VIA SERIALE *
 ***************************************/


void cmdGetStatus() {
  currentState.statusToSerial( true );
}

void cmdGetStatusRA() {
  currentState.statusToSerial( false );
  currentState.resetAccumulators();
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

void cmdIncRotexTermoMin() {
  if (runtimeSettings.rotexTermoMin + 1 < runtimeSettings.rotexTermoMax)
    runtimeSettings.rotexTermoMin++;  

  settingsToSerial(runtimeSettings);
}

void cmdDecRotexTermoMin() {
  if (runtimeSettings.rotexTermoMin - 1 > MIN_ROTEX_TERMO_MIN)
    runtimeSettings.rotexTermoMin--;  

  settingsToSerial(runtimeSettings);
}

void cmdIncRotexTermoMax() {
  if (runtimeSettings.rotexTermoMax - 1 < MAX_ROTEX_TERMO_MAX)
    runtimeSettings.rotexTermoMax++;  

  settingsToSerial(runtimeSettings);
}

void cmdDecRotexTermoMax() {
  if (runtimeSettings.rotexTermoMax - 1 > runtimeSettings.rotexTermoMin)
    runtimeSettings.rotexTermoMax--;  

  settingsToSerial(runtimeSettings);
}

void printHelp() {
  Serial.println(F("Available Commands:"));
  Serial.println(F("  GET     Obtains the status of the system."));
  Serial.println(F("  GET-RA  Same as GET plus some values and Resets all the Accumulatros variables."));
  Serial.println(F("  GET-RS  Returns a json with the runtime settings."));
  Serial.println(F("  LD-RS   Loads settings from the EEPROM. Will revert to defaults if not valid."));
  Serial.println(F("  ST-RS   Saves the runtime Settings."));
  Serial.println(F("  RST     Restores the default Settings."));
  Serial.println(F("  +RTm    Increments rotexTermoMin variable."));
  Serial.println(F("  -RTm    Decrements rotexTermoMin variable."));
  Serial.println(F("  +RTM    Increments rotexTermoMax variable."));
  Serial.println(F("  -RTM    Decrements rotexTermoMax variable."));
  Serial.println(F("  ?       Prints this help screen."));
};

void serialCmdSetup () {
  // Setup dei comandi via Seriale
  SCmd.addCommand( "+RTm", cmdIncRotexTermoMin ); // Incrementa il valore di Rotex TermoMin
  SCmd.addCommand( "+RTM", cmdIncRotexTermoMax ); // Incrementa il valore di Rotex TermoMAX
  SCmd.addCommand( "-RTm", cmdDecRotexTermoMin ); // Decrementa il valore di Rotex TermoMin
  SCmd.addCommand( "-RTM", cmdDecRotexTermoMax ); // Decrementa il valore di Rotex TermoMAX
  SCmd.addCommand( "?", printHelp ); // Stampa un piccolo messaggio di Help
  SCmd.addCommand( "GET", cmdGetStatus ); // Restituisce lo stato delle variabili interne
  SCmd.addCommand( "GET-RA", cmdGetStatusRA ); // Restituisce lo stato delle variabili interne e resetta gli accumulatori
  SCmd.addCommand( "GET-RS", cmdGetRuntimeSettings ); // Restituisce lo stato delle impostazioni
  SCmd.addCommand( "LD-RS", cmdLoadRuntimeSettings ); // Restituisce lo stato delle impostazioni
  SCmd.addCommand( "RST", cmdRestoreSettingsToDefault ); 
  SCmd.addCommand( "ST-RS", cmdSaveRuntimeSettings ); // Restituisce lo stato delle impostazioni
  SCmd.addDefaultHandler( dummyMethod ); // Handler for command that isn't matched (says "What?")

  printHelp();
  Serial.println( F("Ready.") );  
}



// Lettura dei valori
void doReadInputs() {
  currentState.inTermoAmbienteValue = digitalRead( inTermoAmbiente );
  // Serial.print( "inTermoAmbiente=" );
  // Serial.println( inTermoAmbienteValue == HIGH ? ACCESO : SPENTO );

  currentState.inTermoAccumulatoreValue = digitalRead( inTermoAccumulatore );
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
  static float lastValidAinTempCaminoValueCentigradi = 0.0;
  
  int valuesToAverage = 0;

  if (  calcolaIntervallo( currentState.lastTempAcquired, millis() ) >= runtimeSettings.TEMP_SAMPLING_INTERVAL ) {
    currentState.lastTempAcquired = millis();

    if ( tempIndex >= CB_VALUES ) {
      tempIndex = 0;
      currentState.ainTempCaminoValue = 0;
      valuesToAverage = 0;
      for ( char i = 0; i < CB_VALUES; i++ ) {
        if ( i != minIndex && i != maxIndex ) {
          currentState.ainTempCaminoValue += Temps[ i ];
          valuesToAverage ++;
        }
      } 
      
      currentState.ainTempCaminoValue /= valuesToAverage;
      currentState.ainTempCaminoValueCentigradi = (float)getTempFromAin(currentState.ainTempCaminoValue);
      // 2016 10 12 - Compensazione linearizzazione sonda NTC. Dalle ultime misurazioni pare che la lookup legga
      // almeno 4 gradi in più rispetto alla realtà. Il risultato è che la pompa parte quando il camino è ancora troppo freddo
      // causando condensa.
      // 2017 01 12 - Visto l'incendio della canna fumaria, riduciamo l'offset a 2 gradi
      currentState.ainTempCaminoValueCentigradi = currentState.ainTempCaminoValueCentigradi > 2 ? currentState.ainTempCaminoValueCentigradi - 2 : 0 ;
      
      // 2017 01 16 - Per evitare i valori a zero che ogni tanto capitano, faccio in modo che se la lettura analogica 
      //              è zero, prendo l'ultima lettura valida. 
      if (currentState.ainTempCaminoValueCentigradi == 0) {
        currentState.ainTempCaminoValueCentigradi = lastValidAinTempCaminoValueCentigradi;
      } else {
        lastValidAinTempCaminoValueCentigradi = currentState.ainTempCaminoValueCentigradi;
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
    if( currentState.outCaldaiaValue == LOW ? RotexStatus::rotexValues[ RTX_TS ] < sogliaMin : RotexStatus::rotexValues[ RTX_TS ] > sogliaMax ) {
      currentState.outCaldaiaValue == HIGH ? currentState.outCaldaiaValue = LOW : currentState.outCaldaiaValue = HIGH;
    }
  }  else {

    // Gestione dell'accesnione tramite termostato + isteresi temporale
    if ( currentState.inTermoAccumulatoreValue == HIGH ) {
      if ( currentState.outCaldaiaValue == LOW ) {
        currentState.outCaldaiaValue = HIGH;
        currentState.isteLastOutCaldaia_On = millis();
        currentState.isteLastOutCaldaia_On_For = 0;
      }
    }
    if ( currentState.inTermoAccumulatoreValue == LOW ) {
      if ( currentState.outCaldaiaValue == HIGH ) {
        currentState.isteLastOutCaldaia_On_For = calcolaIntervallo( currentState.isteLastOutCaldaia_On, millis() );
        if ( currentState.isteLastOutCaldaia_On_For > runtimeSettings.T_ISTERESI_CALDAIA ) {
          currentState.outCaldaiaValue = LOW;
          currentState.isteLastOutCaldaia_On = 0;
          currentState.isteLastOutCaldaia_On_For = 0;
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
#define PUO_CAMBIARE_STATO ( currentState.outPompaCaminoValue == LOW ? TEMPO_IN_OFF >= TEMPO_MINIMO_PER_STATO : TEMPO_IN_ON >= TEMPO_MINIMO_PER_STATO )
#define ROTEX_DISPONIBILE ( RotexStatus::rotexValues[ RTX_TS ] != 0 )
#define ROTEX_NON_E_DISPONIBILE !ROTEX_DISPONIBILE
#define DELTA_CAMINO_ROTEX ( (int)currentState.ainTempCaminoValueCentigradi - (int)RotexStatus::rotexValues[ RTX_TS ] )
#define T_CAMINO (int)currentState.ainTempCaminoValueCentigradi
#define ACCENDI_POMPA_CAMINO      tOnPompaCamino  = millis(); currentState.outPompaCaminoValue = HIGH;          
#define SPEGNI_POMPA_CAMINO       tOffPompaCamino = millis(); currentState.outPompaCaminoValue = LOW;          
#define POMPA_CAMINO_E_SPENTA     currentState.outPompaCaminoValue == LOW
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

  /* La pompa del riscaldamento è direttamente collegata alla richiesta di calore nel da parte dei termostati ambiente.*/
  currentState.inTermoAmbienteValue     == HIGH ? currentState.outPompaValue   = HIGH : currentState.outPompaValue = LOW;
}

void doManageOverrideTermostatoAmbiente() {
#define SE_OVERRIDE_TERMOSTATO_AMBIENTE_NON_E_ATTIVO    currentState.outOverrideTermoAmbienteValue == LOW

  if (currentState.ainTempCaminoValueCentigradi == 0.0) {
    return;
  }
    
  if (ROTEX_NON_E_DISPONIBILE) {
    if ( SE_OVERRIDE_TERMOSTATO_AMBIENTE_NON_E_ATTIVO ) {
      if (currentState.ainTempCaminoValueCentigradi > runtimeSettings.TInnescoOverrideTermostatoSeRotexNonDisponibile /* Ad Esempio 65 */ ) {
        currentState.outOverrideTermoAmbienteValue = HIGH;    
      }
    } else {
      if (currentState.ainTempCaminoValueCentigradi <= runtimeSettings.TInnescoOverrideTermostatoSeRotexNonDisponibile  /* Ad Esempio 60 */) {
        currentState.outOverrideTermoAmbienteValue = LOW;    
      }  
    }    
    return;
  }

  if (currentState.ainTempCaminoValueCentigradi <= runtimeSettings.TDisinnescoOverrideSeRotexDisponibile /* Ad esempio 60 */) {
    currentState.outOverrideTermoAmbienteValue = LOW;
    return;
  }

  // Se la temperatura è maggiore di 60, supponiamo che il camino stia funzionando
  if ( currentState.outOverrideTermoAmbienteValue == LOW ) {
    if (RotexStatus::rotexValues[RTX_TS] >= runtimeSettings.rotexMaxTempConCamino) {
      currentState.outOverrideTermoAmbienteValue = HIGH;    
    }
  } else {
    if (RotexStatus::rotexValues[RTX_TS] <= runtimeSettings.rotexMinTempConCamino) {
      currentState.outOverrideTermoAmbienteValue = LOW;    
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

  currentState.outPompaCaminoValue      == HIGH ? currentState.outPompaCaminoAccu_On      += Accu_Length : currentState.outPompaCaminoAccu_Off      += Accu_Length;
  currentState.inTermoAmbienteValue     == HIGH ? currentState.inTermoAmbienteAccu_On     += Accu_Length : currentState.inTermoAmbienteAccu_Off     += Accu_Length;
  currentState.inTermoAccumulatoreValue == HIGH ? currentState.inTermoAccumulatoreAccu_On += Accu_Length : currentState.inTermoAccumulatoreAccu_Off += Accu_Length;
  currentState.outPompaValue            == HIGH ? currentState.outPompaAccu_On            += Accu_Length : currentState.outPompaAccu_Off            += Accu_Length;
  currentState.outCaldaiaValue          == HIGH ? currentState.outCaldaiaAccu_On          += Accu_Length : currentState.outCaldaiaAccu_Off          += Accu_Length;
  
  currentState.outOverrideTermoAmbienteValue == HIGH ? currentState.outOverrideTermoAmbienteAccu_On += Accu_Length : currentState.outOverrideTermoAmbienteAccu_Off += Accu_Length;
  
  Accu_LastTS = Accu_ActualTS;
  currentState.rotexP1Accu_On += RotexStatus::rotexValues[ RTX_P1 ] != 0 ?  Accu_Length : 0;
//  timeSinceLastAccuResetMs = calcolaIntervallo( lastAccuResetTime, Accu_ActualTS );
  currentState.timeSinceLastAccuResetMs = currentState.inTermoAmbienteAccu_On + currentState.inTermoAmbienteAccu_Off;

}

// Impostazione dell'output
void doSetOutputs() {
  digitalWrite( outPompaCamino,           currentState.outPompaCaminoValue );   
  digitalWrite( outPompa,                 currentState.outPompaValue   );
  digitalWrite( outCaldaia,               currentState.outCaldaiaValue );
  digitalWrite( outOverrideTermoAmbiente, currentState.outOverrideTermoAmbienteValue );
  // La logica di reset è contenuta nella funzione;
  // doRotexReset();
}


/***********************
 *   METODI PRINCIPALI *
 ***********************/

void setup() {
    // Utilizziamo la seriale per fare un po' di debug.
  Serial.begin(9600);
  
  runtimeSettings = loadSettingsFromEEPROM();
  ioSetup();
  
  serialCmdSetup();
  rotexSerialSetup();
  
  currentState.initVariables();

  currentState.resetAccumulators();

  // imposto il riferimeno analogico agli 1.1 volt interni
  // analogReference( INTERNAL );
  // Imposta il reference 5V
  analogReference( DEFAULT );

}

void loop() {
  currentState.loopStartMillis = millis();
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

  if(calcolaIntervallo(currentState.lastEmittedValues, currentState.loopStartMillis) >= 5000) {
    currentState.lastEmittedValues = currentState.loopStartMillis;
    // statusToSerial(false);
  }
  // 
  SCmd.readSerial();
}
