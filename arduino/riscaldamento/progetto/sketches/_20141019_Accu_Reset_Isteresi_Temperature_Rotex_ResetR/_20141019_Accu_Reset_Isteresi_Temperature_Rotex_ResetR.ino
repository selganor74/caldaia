#include <SPI.h>
#include <SoftwareSerial.h>
#include <SerialCommand.h>

// Numero di campioni memorizzati nel buffer circolare
#define  CB_MAX_VALUES  15
#define  CB_AVG_REBUILD_AFTER 1
#include <CircularBuffer.h>

// Abilita o disabilita il debug tramite seriale
boolean serialDebug;

// Oggetto SerialCommand per gestire i comandi da Seriale
SerialCommand SCmd; 

CircularBuffer cbTempAmbienteCentigradi = CircularBuffer();
CircularBuffer cbRotexTS = CircularBuffer();
CircularBuffer cbRotexPortataTV = CircularBuffer();

// In questa versione:
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
5	12 (MIS0)	Out-5 (M19) 
4	11 (MOSI)	Out-4 (M20) outRotexReset. Invia impulsi di 5 secondi ad un
                                    rele NC per spegnere e riaccendere la centralina ROTEX.
3	10 (SS)		Out-3 (M21) TX_Seriale Rotex <<-- Sebbene non sia usato, viene comunque occupato dalla libreria seriale !
2	9		In-6  (M6)
1	8		In-5  (M5)

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

const char serialRotexRX = 4;       // Pin 4  -> Evaristo In-3 -> M3
const char serialRotexTX = 10;      // Pin 10 -> Evaristo Out-3 -> M21
const char inTermoAmbiente = 3;     // M1 Input Termostati Ambiente
const char inTermoAccumulatore = 2; // M2 Input Termostato Accumulatore
const char outRotexReset = 11;       // Output per reset Rotex. L'output agisce su un Rele' NC
const char ainTempAmbiente = 0 ;    // M9 Input analogico LM35 Temperatura ambiente

const char rotexTermoMin = 40;      // Temperatura di accensione delle caldaia
const char rotexTermoMax = 42;      // Temperatura di spegnimento della caldaia
const char deltaSolare   = 1;       // Quando i pannelli sono in funzione la temperatura di soglia della caldaia (rotexTermoMin) scende di deltaSolare Gradi.
SoftwareSerial rotexSerial = SoftwareSerial(serialRotexRX,serialRotexTX); //Initialize 2nd serial port (rx,tx)

// Millisecondi di isteresi prima che la caldaia possa spegnersi 1h=60*60*1000=3600000  20min = 1200000
const unsigned long T_ISTERESI_CALDAIA = 1200000 ;
// Millisecondi tra un'acquisizione temperature e l'altra.
const unsigned long TEMP_SAMPLING_INTERVAL = 10000 ;
// Millis dell'ultima acquisizione temperature
unsigned long lastTempAcquired;
// Costanti per l'output
#define ACCESO "1"
#define SPENTO "0"

unsigned int  loopStartMillis;
unsigned int  loopLengthMillis;

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

boolean outRotexResetValue;
unsigned long rotexResetCounter;

unsigned long isteLastOutCaldaia_On;
unsigned long isteLastOutCaldaia_On_For;

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
#define ROTEX_MAX_STRING_LEN 48
char rotexLastReadString[ROTEX_MAX_STRING_LEN];

int   ainTempAmbienteValue;
float ainTempAmbienteValueCentigradi;


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
void doRotexReset() {
  static boolean rotexIsResetting = false;
  static unsigned long resetStartedAtMillis = 0;
  const unsigned long resetTimeMs = 10000;
  // N.B.: Se l'output è in logica negata, occorre invertire i valori qui sotto:
#define RESTVALUE LOW
#define ACTIVEVALUE HIGH
  
  // Se siamo in fase di reset ed il tempo di timeout è passato, ritorno a riposo
  if ( rotexIsResetting && ( calcolaIntervallo(resetStartedAtMillis, millis() ) > resetTimeMs ) ) {
    rotexHasFailed = false;
    rotexIsResetting = false;
  }

  // Se abbiamo rilevato uno stato di FAIL e non siamo già in fase di reset, avvio la procedura di reset
  if ( rotexHasFailed && !rotexIsResetting ) {
    rotexResetCounter++;
    rotexIsResetting = true;
    resetStartedAtMillis = millis();
    digitalWrite( outRotexReset, ACTIVEVALUE );
  } 

  // Se non siamo in una condizione di fail e non stiamo resettando, allora teniamo l'output a RIPOSO (RESTVALUE)
  // N.B.: Questa sezione di codice gira anche subito dopo che si è verificata la condizione di TIMEOUT passato.
  if ( !rotexHasFailed && !rotexIsResetting ) {
        digitalWrite( outRotexReset, RESTVALUE );   
  }
}

// Genera un impoulso di lunghezza "milliseconds"
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
  outPompaAccu_On=0;
  outPompaAccu_Off=0;
  outCaldaiaAccu_On=0;
  outCaldaiaAccu_Off=0;
  inTermoAmbienteAccu_On=0;
  inTermoAmbienteAccu_Off=0;
  inTermoAccumulatoreAccu_On=0;
  inTermoAccumulatoreAccu_Off=0;

  rotexResetCounter = 0;

  lastAccuResetTime = millis();
}

void initVariables() {
  outPompaValue = LOW;

  outCaldaiaValue = LOW;

  outRotexResetValue = HIGH;

  inTermoAmbienteValue=false;

  inTermoAccumulatoreValue=false;

  rotexResetCounter = 0;

  ainTempAmbienteValue=0;
  ainTempAmbienteValueCentigradi=0.0;
  lastTempAcquired = 0;
  rotexLastRead = 0;
  rotexPortataTV = 0.0;
}


boolean waitForRotexData() {
#define ROTEX_DATA_TIMEOUT 200
  unsigned long  elapsed;
  unsigned long  started;
  elapsed = 0;
  started = millis();
  
  while ( !rotexSerial.available() && elapsed < ROTEX_DATA_TIMEOUT ) {
    elapsed = calcolaIntervallo( started, millis() );
  }
  if ( !rotexSerial.available() ) {
    return false;
  }
  return true;
}

// Appena accesa la centralina R3 restituisce questa stringa:
//            SOLARIS R3 V.3.1 May 30 2007 14:37:58 ROTEX GmbH Ser.-Nr: 000001******
// Successivamente:
// L'output seriale della R3 è un record con i valori separati da ';'
//            HA;BK;P1 /%;P2;TK /Ã¸C;TR /Ã¸C;TS /Ã¸C;TV /Ã¸C;V /l/min
boolean doReadRotex() {
  boolean tmp_return;
  int i;
  int alarmIdx;
  String currVal;
  unsigned char buf[10];
  char inchar;
  boolean foundSemicolon;
  static int currentCharIndex;
  i = 0;
  alarmIdx = 0;
  tmp_return = false;
  currVal = "";
  foundSemicolon = false;
  currentCharIndex = 0;
  while ( rotexSerial.available() ) {
    inchar = rotexSerial.read();
    // Serial.print( inchar );
    // Esclude i ritorni a capo dalla stringa letta dal rotex
    if ( currentCharIndex < ROTEX_MAX_STRING_LEN - 1  and inchar != 13 and inchar != 10) {
      rotexLastReadString[ currentCharIndex ] = inchar;
      currentCharIndex++;
      // Imposta il terminatore null.
      rotexLastReadString[ currentCharIndex ] = 0;
    }
    inchar == ',' ? inchar = '.' : inchar = inchar;
    if ( inchar == ';' || ( ( inchar == 13 ) && ( foundSemicolon == true ) ) ) {
      foundSemicolon = true;
      currVal.getBytes( &buf[0], 10 );
      if( i == 9 ) {
        rotexHasFailed = 0;
        for ( alarmIdx = 0; alarmIdx < 10 && buf[alarmIdx] != 0; alarmIdx++ ) {
          switch ( buf[alarmIdx] ) {
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
        rotexPortataTV = atof( (char *)&buf[0] );
        //Serial.println (rotexPortataTV);
      } else {
        rotexValues[ i ] = atoi( (char *)&buf[0] );
        //Serial.println (rotexValues[i]);
      }
      currVal = "";
      i++;
    } else {
      currVal = currVal + inchar;
    }
    if ( i == 11 ) {
     tmp_return = true;
     cbRotexTS.addValue( rotexValues[RTX_TS] );
     cbRotexPortataTV.addValue( rotexPortataTV );
     rotexLastRead = millis();
    } else {
      tmp_return = false;
      if ( i != 0 ) rotexLastRead = 0 ;
    }
  }
  return tmp_return;
}

/***************************************
 * DEFINIZIONE DEI COMANDI VIA SERIALE *
 ***************************************/

void cmdGetStatus () {
  Serial.println( "{" );
  Serial.print( "  \"loopStartMillis\": " );
  Serial.print( loopStartMillis );
  Serial.println( "," );
  Serial.print( "  \"outPompaValue\": " );
  Serial.print( outPompaValue );
  Serial.println( "," );
  Serial.print( "  \"outPompaAccu_On\": " );
  Serial.print( outPompaAccu_On );
  Serial.println( "," );
  Serial.print( "  \"outPompaAccu_Off\": " );
  Serial.print( outPompaAccu_Off );
  Serial.println( "," );

  Serial.print( "  \"outCaldaiaValue\": " );
  Serial.print( outCaldaiaValue );
  Serial.println( "," );
  Serial.print( "  \"outCaldaiaAccu_On\": " );
  Serial.print( outCaldaiaAccu_On );
  Serial.println( "," );
  Serial.print( "  \"outCaldaiaAccu_Off\": " );
  Serial.print( outCaldaiaAccu_Off );
  Serial.println( "," );
  
  Serial.print( "  \"inTermoAmbienteValue\": " );
  Serial.print( inTermoAmbienteValue );
  Serial.println( "," );
  Serial.print( "  \"inTermoAmbienteAccu_On\": " );
  Serial.print( inTermoAmbienteAccu_On );
  Serial.println( "," );
  Serial.print( "  \"inTermoAmbienteAccu_Off\": " );
  Serial.print( inTermoAmbienteAccu_Off );
  Serial.println( "," );
  
  Serial.print( "  \"inTermoAccumulatoreValue\": " );
  Serial.print(  inTermoAccumulatoreValue);
  Serial.println( "," );
  Serial.print( "  \"inTermoAccumulatoreAccu_On\": " );
  Serial.print( inTermoAccumulatoreAccu_On );
  Serial.println( "," );
  Serial.print( "  \"inTermoAccumulatoreAccu_Off\": " );
  Serial.print( inTermoAccumulatoreAccu_Off );
  Serial.println( "," );
 
  Serial.print( "  \"timeSinceLastAccuResetMs\": " );
  Serial.print( timeSinceLastAccuResetMs );  
  Serial.println( "," );
  Serial.print( "  \"lastAccuResetTime\": " );
  Serial.print( lastAccuResetTime );
  Serial.println( "," );
  
  Serial.print( "  \"outRotexResetValue\": " );
  Serial.print( outRotexResetValue );
  Serial.println( "," );
  Serial.print( "  \"rotexResetCounter\": " );
  Serial.print( rotexResetCounter );
  Serial.println( "," );
  
  Serial.print( "  \"isteLastOutCaldaia_On\": " );
  Serial.print( isteLastOutCaldaia_On );
  Serial.println( "," );
  Serial.print( "  \"isteLastOutCaldaia_On_For\": " );
  Serial.print( isteLastOutCaldaia_On_For );
  Serial.println( "," );
    
    
    
  Serial.print( "  \"rotexHA\": " );
  Serial.print( rotexValues[ RTX_HA ] );
  Serial.println( "," );
  
  Serial.print( "  \"rotexBK\": " );
  Serial.print( rotexValues[ RTX_BK ] );
  Serial.println( "," );
  
  Serial.print( "  \"rotexP1\": " );
  Serial.print( rotexValues[ RTX_P1 ] );
  Serial.println( "," );
  
  Serial.print( "  \"rotexP2\": " );
  Serial.print( rotexValues[ RTX_P2 ] );
  Serial.println( "," );
   
  Serial.print( "  \"rotexTK\": " );
  Serial.print( rotexValues[ RTX_TK ] );
  Serial.println( "," );

  Serial.print( "  \"rotexTR\": " );
  Serial.print( rotexValues[ RTX_TR ] );
  Serial.println( "," );

  Serial.print( "  \"rotexTS\": " );
  Serial.print( rotexValues[ RTX_TS ] );
  Serial.println( "," );

  Serial.print( "  \"rotexTV\": " );
  Serial.print( rotexValues[ RTX_TV ] );
  Serial.println( "," );

  Serial.print( "  \"rotexPWR\": " );
  Serial.print( rotexValues[ RTX_PWR ] );
  Serial.println( "," );

  Serial.print( "  \"rotexQT\": " );
  Serial.print( rotexValues[ RTX_QT ] );
  Serial.println( "," );



  Serial.print( "  \"rotexPortataTV\": " );
  Serial.print( rotexPortataTV );
  Serial.println( "," );
  
  Serial.print( "  \"rotexLastRead\": " );
  Serial.print( rotexLastRead );
  Serial.println( "," );
  
  Serial.print( "  \"rotexLastReadString\": \"" );
  Serial.print( rotexLastReadString );
  Serial.println( "\"," );
  
  Serial.print( "  \"ainTempAmbienteValueCentigradi\": " );
  Serial.print( ainTempAmbienteValueCentigradi );
  Serial.println( "," );
  
  Serial.print( "  \"rotexHasFailed\": " );
  Serial.print( rotexHasFailed );
  Serial.println( "," );
  
  Serial.println( "  \"cbTempAmbienteCentigradi\": {" ); 
  Serial.print( "    \"current_avg\": " ); 
  Serial.print( cbTempAmbienteCentigradi.current_avg );
  Serial.println ( "," );
  Serial.print( "    \"current_min\": " ); 
  Serial.print( cbTempAmbienteCentigradi.current_min );
  Serial.println ( "," );
  Serial.print( "    \"current_max\": " ); 
  Serial.println( cbTempAmbienteCentigradi.current_max );
  Serial.println( "  }," ); 

  Serial.println( "  \"cbRotexTS\": {" );
  Serial.print( "    \"current_avg\": " ); 
  Serial.print( cbRotexTS.current_avg );
  Serial.println ( "," );
  Serial.print( "    \"current_min\": " ); 
  Serial.print( cbRotexTS.current_min );
  Serial.println ( "," );
  Serial.print( "    \"current_max\": " ); 
  Serial.println( cbRotexTS.current_max );
  Serial.println( "  }," ); 

  Serial.println( "  \"cbRotexPortataTV\": {" );
  Serial.print( "    \"current_avg\": " ); 
  Serial.print( cbRotexPortataTV.current_avg );
  Serial.println ( "," );
  Serial.print( "    \"current_min\": " ); 
  Serial.print( cbRotexPortataTV.current_min );
  Serial.println ( "," );
  Serial.print( "    \"current_max\": " ); 
  Serial.println( cbRotexPortataTV.current_max );
  Serial.println( "  }" ); 
  Serial.println( "}" ); 
  
}

void cmdUnrecognized() {
  Serial.println( "Unrecognized Command." );
}

void serialCmdSetup () {
  // Setup dei comandi via Seriale
  SCmd.addCommand( "GETSTATUS", cmdGetStatus ); // Restituisce lo stato delle variabili interne
  SCmd.addDefaultHandler( cmdUnrecognized ); // Handler for command that isn't matched (says "What?")
  Serial.println( "Ready" );
}

void ioSetup() {
  pinMode( outPompa, OUTPUT );
  pinMode( outCaldaia, OUTPUT );
  pinMode( inTermoAmbiente, INPUT );
  pinMode( outRotexReset, OUTPUT );
  pinMode( inTermoAccumulatore, INPUT );
  // pinMode( ainTempAmbiente, INPUT );
}

// Lettura dei valori
void doReadInputs() {
  inTermoAmbienteValue = digitalRead( inTermoAmbiente );
  // Serial.print( "inTermoAmbiente=" );
  // Serial.println( inTermoAmbienteValue == HIGH ? ACCESO : SPENTO );

  inTermoAccumulatoreValue = digitalRead( inTermoAccumulatore );
  // Serial.print( "inTermoAccumulatore=" );
  // Serial.println( inTermoAccumulatoreValue == HIGH ? ACCESO : SPENTO );
  if ( lastTempAcquired == 0 || calcolaIntervallo( lastTempAcquired, millis() ) >= TEMP_SAMPLING_INTERVAL ) {
    lastTempAcquired = millis();
    ainTempAmbienteValue = analogRead( ainTempAmbiente );
    ainTempAmbienteValueCentigradi = 1.94 * ( 1.1 / 1023.0 ) * (float)ainTempAmbienteValue / 0.01;
    cbTempAmbienteCentigradi.addValue( ainTempAmbienteValueCentigradi );
  }
  if (serialDebug) {
    Serial.print( "cbTempAmbienteCentigradiAvg: " );
    Serial.println( cbTempAmbienteCentigradi.current_avg );
  }
    // letturaRotex
  if (doReadRotex()) {
    // Serial.println( rotexLastReadString );
  };
}

// Elaborazione dei valori letti
void doCrunchInputs() {
  // Elaborazione degli output
  //* Gestione Caldaia tramite cbRotexTS usato come termostato
  char sogliaMin;
  char sogliaMax;

  if ( cbRotexTS.total_values > 0 ) {
    sogliaMin = rotexTermoMin - ( cbRotexPortataTV.current_avg != 0 ? deltaSolare : 0 );
    sogliaMax = rotexTermoMax - ( cbRotexPortataTV.current_avg != 0 ? deltaSolare : 0 );
    if( outCaldaiaValue == LOW ? cbRotexTS.current_avg <= sogliaMin : cbRotexTS.current_avg >= sogliaMax ) {
      outCaldaiaValue == HIGH ? outCaldaiaValue = LOW : outCaldaiaValue = HIGH;
    }
  } else {
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
  //*/
  /* La pompa del riscaldamento è direttamente collegata alla richiesta di calore nel da parte dei termostati ambiente.*/
  inTermoAmbienteValue     == HIGH ? outPompaValue   = HIGH : outPompaValue = LOW;
}

void doManageAccumulators() {
  unsigned long Accu_ActualTS;
  unsigned long Accu_Length;
  static unsigned long Accu_LastTS = 0;

  if (Accu_LastTS == 0 ) Accu_LastTS = millis();

  Accu_ActualTS = millis();
  Accu_Length = Accu_ActualTS >= Accu_LastTS ? Accu_ActualTS - Accu_LastTS : Accu_ActualTS + (0-1-Accu_LastTS);

  inTermoAmbienteValue     == HIGH ? inTermoAmbienteAccu_On     += Accu_Length : inTermoAmbienteAccu_Off     += Accu_Length;
  inTermoAccumulatoreValue == HIGH ? inTermoAccumulatoreAccu_On += Accu_Length : inTermoAccumulatoreAccu_Off += Accu_Length;
  outPompaValue            == HIGH ? outPompaAccu_On            += Accu_Length : outPompaAccu_Off            += Accu_Length;
  outCaldaiaValue          == HIGH ? outCaldaiaAccu_On          += Accu_Length : outCaldaiaAccu_Off          += Accu_Length;

  Accu_LastTS = Accu_ActualTS;

  timeSinceLastAccuResetMs = calcolaIntervallo( lastAccuResetTime, millis() );

}

// Impostazione dell'output
void doSetOutputs() {
  digitalWrite( outPompa,   outPompaValue   );
  digitalWrite( outCaldaia, outCaldaiaValue );

  // La logica di reset è contenuta nella funzione;
  doRotexReset();

}


/***********************
 *   METODI PRINCIPALI *
 ***********************/

void setup() {
  // Utilizziamo la seriale per fare un po' di debug.
  Serial.begin(9600);
  rotexSerial.begin(9600);
  serialDebug = false;

  ioSetup();

  initVariables();

  resetAccumulators();

  // imposto il riferimeno analogico agli 1.1 volt interni
  analogReference( INTERNAL );

  serialCmdSetup();
}

void loop() {
  loopStartMillis = millis();
  // lettura delle variabili.
  doReadInputs();
  // elaborazione degli ingressi
  doCrunchInputs();
  // impostazione degli output
  doSetOutputs();
  // gestione degli accumulatori
  doManageAccumulators();
  // Processa i comandi in arrivo dalla seriale
  SCmd.readSerial();
}

