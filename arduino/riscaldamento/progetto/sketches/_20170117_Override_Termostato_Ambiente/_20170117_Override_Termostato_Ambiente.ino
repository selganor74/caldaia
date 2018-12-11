#include <SPI.h>
#include <SoftwareSerial.h>
#include "SerialCommand.h"

#include "ntcTempLookup.c"
#include "Settings.cpp"

// Numero di campioni memorizzati nel buffer circolare
#define  CB_VALUES  5

//..#define  CB_AVG_REBUILD_AFTER 1
//..#include <CircularBuffer.h>

Settings currentSettings;

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
// const int outEthReset = 12;  // M19 Pin per reset EthShield - Sembrerebbe utilizzato dall'ethernet shield
// vedi: http://www.arduino.cc/cgi-bin/yabb2/YaBB.pl?num=1286370597/8

// PIN SETUP
/*
J3    Arduino	     Evaristo
8	AREF		-
7	GND		-
6	13 (SCK)	Out-6 (M18) 
5	12 (MIS0)	Out-5 (M19) outPompaCamino. Comanda la pompa del camino.
4	11 (MOSI)	Out-4 (M20) outRotexReset. Invia impulsi di 5 secondi ad un
                                    rele NC per spegnere e riaccendere la centralina ROTEX.
                                    [2017 01 17 - Sostituito dall'override del Termostato Ambiente]
                                    outOverrideTermoAmbiente [Normalmente Aperto]
3	10 (SS)		Out-3 (M21) TX_Seriale Rotex <<-- Sebbene non sia usato, viene comunque occupato dalla libreria seriale !
2	9		In-6  (M6)
1	8		In-5  (M5)

                        In-   (M9) ain ingresso temperatura camino tramite NTC. 

J1
8	7		In-4  (M4) (TP9) 
7	6		Out-2 (M22) Relay Pompa
6	5		Out-1 (M23) Relay Caldaia
5	4		In-3  (M3) RX_Seriale Rotex
4	3		In-2  (M2) Termostato Ambiente
3	2		In-1  (M1) Termostato Accumulatore 
2	1 (TXD)
1	0 (RXD)
*/
const int outPompa = 5;      // M22 Relay Pompa. M22
const int outCaldaia = 6;    // M23 Relay Caldaia. M23
// Utilizzando lo shield ETHERNET i pin 11, 12, 13 vengono utilizzati per gestire la
// scheda ethernet stessa quindi non devono essere utilizzati.
const char outPompaCamino = 12;     // Pin 12 -> Evaristo Out-5 -> M19
const char serialRotexRX = 4;       // Pin 4  -> Evaristo In-3 -> M3
const char serialRotexTX = 10;      // Pin 10 -> Evaristo Out-3 -> M21
const char inTermoAmbiente = 3;     // M1 Input Termostati Ambiente
const char inTermoAccumulatore = 2; // M2 Input Termostato Accumulatore
// const char outRotexReset = 11;       // Output per reset Rotex. L'output agisce su un Rele' NC
const char outOverrideTermoAmbiente = 11; // Output per Override Termostati Ambiente, [Sostituisce il relay di reset del Rotex]
// const char ainTempCamino = 0 ;    // M9 Input analogico NTC Temperatura Camino 
const char ainTempCamino = 1 ;    // M10 Input analogico NTC Temperatura Camino 

// 2017 01 12 - Visto che per un po' il camino non si potrà usare, dobbiamo tenere più alta la temperatura nell'accumulo.
const char rotexTermoMin = 43;      // Temperatura di accensione delle caldaia
const char rotexTermoMax = 45;      // Temperatura di spegnimento della caldaia
const char deltaSolare   = 1;       // Quando i pannelli sono in funzione la temperatura di soglia della caldaia (rotexTermoMin) scende di deltaSolare Gradi.
const char rotexMaxTempConCamino = 71; // Se la temperatura dell'accumulo rotex è maggiore o uguale a rotexMaxTempConCamino viene attivato l'override 
                                       // del termostato ambiente mandando in circolo in impianto.
const char rotexMinTempConCamino = 69; // L'override viene sganciato quando la temperatura dell'accumulo scende sotto rotexMinTempConCamino.

SoftwareSerial rotexSerial = SoftwareSerial(serialRotexRX,serialRotexTX); //Initialize 2nd serial port (rx,tx)

// Oggetto SerialCommand per gestire i comandi da Seriale
SerialCommand SCmd = SerialCommand(); 
SerialCommand SCmdRotex = SerialCommand( rotexSerial );

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

// Variabile usate dal sistema Rotex
unsigned long rotexHasFailed;
int rotexValues[12];
#define RTX_HA     0
#define RTX_BK     1
#define RTX_P1     2
#define RTX_P2     3
#define RTX_TK     4
#define RTX_TR     5
#define RTX_TS     6
#define RTX_TV     7
#define RTX_PWR    8 /* Non usato vedi variabile PWR*/
#define RTX_QT     9 /* ??? */
float rotexPortataTV;
unsigned long rotexLastRead;
#define ROTEX_MAX_STRING_LEN 36
char rotexLastReadString[ROTEX_MAX_STRING_LEN];

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

// Esegue il reset della centralina rotex tenendola spenta per 10 secondi
// deve essere chiamata finché rotexHasFailed non torna a 0
//void doRotexReset() {
//  static boolean rotexIsResetting = false;
//  static unsigned long resetStartedAtMillis = 0;
//  const unsigned long resetTimeMs = 10000;
//  // N.B.: Se l'output è in logica negata, occorre invertire i valori qui sotto:
//#define RESTVALUE LOW
//#define ACTIVEVALUE HIGH
//  
//  // Se siamo in fase di reset ed il tempo di timeout è passato, ritorno a riposo
//  if ( rotexIsResetting && ( calcolaIntervallo(resetStartedAtMillis, millis() ) > resetTimeMs ) ) {
//    rotexHasFailed = false;
//    rotexIsResetting = false;
//  }
//
//  // Se abbiamo rilevato uno stato di FAIL e non siamo già in fase di reset, avvio la procedura di reset
//  if ( rotexHasFailed && !rotexIsResetting ) {
//    rotexResetCounter++;
//    rotexIsResetting = true;
//    resetStartedAtMillis = millis();
//    digitalWrite( outRotexReset, ACTIVEVALUE );
//  } 
//
//  // Se non siamo in una condizione di fail e non stiamo resettando, allora teniamo l'output a RIPOSO (RESTVALUE)
//  // N.B.: Questa sezione di codice gira anche subito dopo che si è verificata la condizione di TIMEOUT passato.
//  if ( !rotexHasFailed && !rotexIsResetting ) {
//        digitalWrite( outRotexReset, RESTVALUE );   
//  }
//}

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
  rotexLastRead = 0;
  rotexPortataTV = 0.0;
}


// Appena accesa la centralina R3 restituisce questa stringa:
//            SOLARIS R3 V.3.1 May 30 2007 14:37:58 ROTEX GmbH Ser.-Nr: 000001******
// Successivamente:
// L'output seriale della R3 è un record con i valori separati da ';'
//            HA;BK;P1 /%;P2;TK /Ã¸C;TR /Ã¸C;TS /Ã¸C;TV /Ã¸C;V /l/min
void doReadRotex() {
  boolean tmp_return;
  int i;
  int j;
  int alarmIdx;
#define MAX_CURR_VAL_LEN 11 
  char currVal[MAX_CURR_VAL_LEN];
  int currValIndex = 0;

  char inchar;
  static int currentCharIndex;

  boolean foundSemicolon;

  i = 0;
  j = 0;
  alarmIdx = 0;
  tmp_return = false;

  foundSemicolon = false;
  currentCharIndex = 0;
  
#define RESET_CURR_VAL  currValIndex=0; for ( j = 0; j < MAX_CURR_VAL_LEN; j++ ) { currVal[j] = 0; }
  
  RESET_CURR_VAL;
  
  char commandSent[ROTEX_MAX_STRING_LEN];
  
  SCmdRotex.getBuffer( commandSent, ROTEX_MAX_STRING_LEN - 1 );
  strncpy( rotexLastReadString, commandSent, ROTEX_MAX_STRING_LEN - 1 );
  // Serial.println(commandSent);
  while ( commandSent[ currentCharIndex ] != 0 && ( currentCharIndex < ROTEX_MAX_STRING_LEN - 1 ) ) {
    inchar = commandSent[ currentCharIndex ];
    currentCharIndex++;
    // Serial.print( inchar );
    // Esclude i ritorni a capo dalla stringa letta dal rotex
    inchar == ',' ? inchar = '.' : inchar = inchar;
    
    // Serial.print( inchar ) ;
    // Serial.print( ":" ) ;
    // Serial.println( i ) ;
    if ( inchar == ';' || ( ( inchar == 0 ) && ( foundSemicolon == true ) ) ) {
      foundSemicolon = true;
      
      if( i == 9 ) {
        rotexHasFailed = 0;
        for ( alarmIdx = 0; alarmIdx < 10 && currVal[alarmIdx] != 0; alarmIdx++ ) {
          switch ( currVal[alarmIdx] ) {
            case 'F':
              rotexHasFailed = 1 ;
              break;
            case 'K': // Temperatura pannelli troppo alta
              break;
            case 'S': // ???
              break;
          }
        }
      }
      if( i == 8 ) { // se i == 8 cioè per la potenza istantanea
        rotexPortataTV = atof( (char *)currVal );
        //Serial.println (rotexPortataTV);
        //.. cbRotexPortataTV.addValue( rotexPortataTV );
        rotexLastRead = millis();
      } else {
        rotexValues[ i ] = atoi( (char *)currVal );
        switch (i) {
          case RTX_TS: 
            //.. cbRotexTS.addValue( rotexValues[RTX_TS] );
            break;
        }
        //Serial.println (rotexValues[i]);
      }
      RESET_CURR_VAL;
      i++;
    } else {
      currVal[currValIndex] = inchar;
      currValIndex++;
      // Serial.println( currVal );
    }   
     
  }
}

/***************************************
 * DEFINIZIONE DEI COMANDI VIA SERIALE *
 ***************************************/
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
  Serial.print( rotexValues[ RTX_HA ] );
  Serial.println( F(",") );
  
  Serial.print( F("  \"rotexBK\": ") );
  Serial.print( rotexValues[ RTX_BK ] );
  Serial.println( F(",") );
  
  Serial.print( F("  \"rotexP1\": " ));
  Serial.print( rotexValues[ RTX_P1 ] );
  Serial.println( F(",") );
  
  Serial.print( F("  \"rotexP2\": ") );
  Serial.print( rotexValues[ RTX_P2 ] );
  Serial.println( F("," ));
   
  Serial.print( F("  \"rotexTK\": ") );
  Serial.print( rotexValues[ RTX_TK ] );
  Serial.println( F(",") );

  Serial.print( F("  \"rotexTR\": ") );
  Serial.print( rotexValues[ RTX_TR ] );
  Serial.println( F(",") );

  Serial.print( F("  \"rotexTS\": ") );
  Serial.print( rotexValues[ RTX_TS ] );
  Serial.println( F(",") );

  Serial.print( F("  \"rotexTV\": ") );
  Serial.print( rotexValues[ RTX_TV ] );
  Serial.println( F(",") );

  Serial.print( F("  \"rotexPWR\": ") );
  Serial.print( rotexValues[ RTX_PWR ] );
  Serial.println( F(",") );

  Serial.print( F("  \"rotexQT\": ") );
  Serial.print( rotexValues[ RTX_QT ] );
  Serial.println( F(",") );



  Serial.print( F("  \"rotexPortataTV\": ") );
  Serial.print( rotexPortataTV );
  Serial.println( F(",") );
  
  if (!compactVersion) {
  Serial.print( F("  \"rotexLastRead\": ") );
  Serial.print( rotexLastRead );
  Serial.println( F(",") );
  }  
  Serial.print( F("  \"rotexLastReadString\": \"") );
  Serial.print( rotexLastReadString );
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
  Serial.println( rotexHasFailed );
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
  settingsToSerial();
}

void dummyMethod() {
  Serial.println(F("What?!"));
  return;
}

void serialCmdSetup () {


  // Setup dei comandi via Seriale
  SCmd.addCommand( "GET", cmdGetStatus ); // Restituisce lo stato delle variabili interne
  SCmd.addCommand( "GET-RA", cmdGetStatusRA ); // Restituisce lo stato delle variabili interne e resetta gli accumulatori
  SCmd.addCommand( "GET-RS", cmdGetRuntimeSettings ); // Restituisce lo stato delle impostazioni
  SCmd.addCommand( "HELP", printHelp ); // Stampa un piccolo messaggio di Help
  SCmd.addDefaultHandler( dummyMethod ); // Handler for command that isn't matched (says "What?")
  
  // Legge effettivamente l'output dell Rotex
  SCmdRotex.addDefaultHandler( doReadRotex );

  Serial.println( F("Ready.") );  
}

void printHelp() {
  Serial.println(F("Available Commands:"));
  Serial.println(F("  GET      Obtains the status of the system."));
  Serial.println(F("  GET-RA   Same as GET plus some values and Resets all the Accumulatros variables."));
  Serial.println(F("  GET-RS   Returns a json with the runtime settings."));
  Serial.println(F("  HELP     Print these informations."));
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
  if ( rotexValues[ RTX_TS ] != 0 ) {
    sogliaMin = rotexTermoMin; // - ( cbRotexPortataTV.current_avg != 0 ? deltaSolare : 0 );
    sogliaMax = rotexTermoMax; // - ( cbRotexPortataTV.current_avg != 0 ? deltaSolare : 0 );
    if( outCaldaiaValue == LOW ? rotexValues[ RTX_TS ] < sogliaMin : rotexValues[ RTX_TS ] > sogliaMax ) {
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
#define ROTEX_DISPONIBILE ( rotexValues[ RTX_TS ] != 0 )
#define DELTA_CAMINO_ROTEX ( (int)ainTempCaminoValueCentigradi - (int)rotexValues[ RTX_TS ] )
#define T_CAMINO (int)ainTempCaminoValueCentigradi
#define ACCENDI_POMPA_CAMINO      tOnPompaCamino  = millis(); outPompaCaminoValue = HIGH;          
#define SPEGNI_POMPA_CAMINO       tOffPompaCamino = millis(); outPompaCaminoValue = LOW;          

  //*
  if ( PUO_CAMBIARE_STATO ) {
    if ( outPompaCaminoValue == LOW ) {
      if ( ROTEX_DISPONIBILE ) {
        if ( DELTA_CAMINO_ROTEX >= 3 || T_CAMINO > 62 ) {
          ACCENDI_POMPA_CAMINO
        }
      }
      if ( !ROTEX_DISPONIBILE ) {
        if ( T_CAMINO >= 55 ) {
          ACCENDI_POMPA_CAMINO
        }  
      } 
    } else {   // if ( outPompaCaminoValue == LOW )
      if ( ROTEX_DISPONIBILE ) {
        if ( DELTA_CAMINO_ROTEX < 3 ) {
          SPEGNI_POMPA_CAMINO
        }
      }
      if ( !ROTEX_DISPONIBILE ) {
        if ( T_CAMINO < 53 ) {
          SPEGNI_POMPA_CAMINO
        }         
      }     
    }          // if ( outPompaCaminoValue == LOW )
  }          // if ( PUO_CAMBIARE_STATO )
  //*/
  /*
  if ( PUO_CAMBIARE_STATO ) {
    if ( outPompaCaminoValue == LOW ) { // La pompa è spenta vediamo se possiamo accenderla
      if (rotexValues[ RTX_TS ] != 0 ) {
        if ((int)ainTempCaminoValueCentigradi - (int)rotexValues[ RTX_TS ] >= 5 ) {
          if ( ainTempCaminoValueCentigradi <= 53.0 ) {
            if ( rotexValues[ RTX_TS ] < 45 ) {
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
      if (rotexValues[ RTX_TS ] != 0 ) {
        if ( ainTempCaminoValueCentigradi <= 53.0 ) {
          if ( ( rotexValues[ RTX_TS ] >= 45 ) || ( (int)ainTempCaminoValueCentigradi - (int)rotexValues[ RTX_TS ] < 5 ) ) {
            tOffPompaCamino = millis();
            outPompaCaminoValue = LOW;
          }
        } else {
          if (((int)ainTempCaminoValueCentigradi - (int)rotexValues[ RTX_TS ] < 3 ) && ( ainTempCaminoValueCentigradi <= 59.0 ) ) {
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
  if (ainTempCaminoValueCentigradi == 0.0) {
    return;
  }
  
  if (rotexValues[RTX_TS] == 0) {
    return;
  }
  
  if (ainTempCaminoValueCentigradi < 40.0) {
    outOverrideTermoAmbienteValue = LOW;
    return;
  }
  
  // Se la temperatura è maggiore di 40, supponiamo che il camino stia funzionando
  if ( outOverrideTermoAmbienteValue == LOW ) {
    if (rotexValues[RTX_TS] >= rotexMaxTempConCamino) {
      outOverrideTermoAmbienteValue = HIGH;    
    }
  } else {
    if (rotexValues[RTX_TS] <= rotexMinTempConCamino) {
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
  rotexP1Accu_On += rotexValues[ RTX_P1 ] != 0 ?  Accu_Length : 0;
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
  rotexSerial.begin(9600);  serialDebug = false;

  ioSetup();
  
  serialCmdSetup();
  
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
  SCmdRotex.readSerial();
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
