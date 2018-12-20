#include <SoftwareSerial.h>
#include "SerialCommand.h"
#include "HwSetup.h"

#include "Rotex.h"

static SoftwareSerial   rotexSerial = SoftwareSerial(serialRotexRX,serialRotexTX); //Initialize 2nd serial port (rx,tx)

static SerialCommand SCmdRotex = SerialCommand( rotexSerial );

unsigned long RotexStatus::rotexHasFailed = 0;
int RotexStatus::rotexValues[12];
float RotexStatus::rotexPortataTV = 0.0f;
unsigned long RotexStatus::rotexLastRead = 0;
char RotexStatus::rotexLastReadString[ROTEX_MAX_STRING_LEN];


#define MAX_CURR_VAL_LEN 11 
#define RESET_CURR_VAL  currValIndex=0; for ( j = 0; j < MAX_CURR_VAL_LEN; j++ ) { currVal[j] = 0; }

// Legge effettivamente l'output dell Rotex
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
    
  RESET_CURR_VAL;
  
  char commandSent[ROTEX_MAX_STRING_LEN];
  
  SCmdRotex.getBuffer( commandSent, ROTEX_MAX_STRING_LEN - 1 );
  strncpy( RotexStatus::rotexLastReadString, commandSent, ROTEX_MAX_STRING_LEN - 1 );
  // Serial.println(commandSent);
  while ( commandSent[ currentCharIndex ] != 0 && ( currentCharIndex < ROTEX_MAX_STRING_LEN - 1 ) ) {
    inchar = commandSent[ currentCharIndex ];
    currentCharIndex++;
    inchar == ',' ? inchar = '.' : inchar = inchar;
    
    if ( inchar == ';' || ( ( inchar == 0 ) && ( foundSemicolon == true ) ) ) {
      foundSemicolon = true;
      
      if( i == 9 ) {
        RotexStatus::rotexHasFailed = 0;
        for ( alarmIdx = 0; alarmIdx < 10 && currVal[alarmIdx] != 0; alarmIdx++ ) {
          switch ( currVal[alarmIdx] ) {
            case 'F':
              RotexStatus::rotexHasFailed = 1 ;
              break;
            case 'K': // Temperatura pannelli troppo alta
              break;
            case 'S': // ???
              break;
          }
        }
      }
      if( i == 8 ) { // se i == 8 cioè per la potenza istantanea
        RotexStatus::rotexPortataTV = atof( (char *)currVal );
        RotexStatus::rotexLastRead = millis();
      } else {
        RotexStatus::rotexValues[ i ] = atoi( (char *)currVal );
        switch (i) {
          case RTX_TS: 
            break;
        }
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

void rotexSerialSetup() {
  rotexSerial.begin(9600); 
  
  SCmdRotex.addDefaultHandler( doReadRotex );
}

void rotexReadSerial() {
  SCmdRotex.readSerial();
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
//    RotexStatus::rotexHasFailed = false;
//    rotexIsResetting = false;
//  }
//
//  // Se abbiamo rilevato uno stato di FAIL e non siamo già in fase di reset, avvio la procedura di reset
//  if ( RotexStatus::rotexHasFailed && !rotexIsResetting ) {
//    rotexResetCounter++;
//    rotexIsResetting = true;
//    resetStartedAtMillis = millis();
//    digitalWrite( outRotexReset, ACTIVEVALUE );
//  } 
//
//  // Se non siamo in una condizione di fail e non stiamo resettando, allora teniamo l'output a RIPOSO (RESTVALUE)
//  // N.B.: Questa sezione di codice gira anche subito dopo che si è verificata la condizione di TIMEOUT passato.
//  if ( !RotexStatus::rotexHasFailed && !rotexIsResetting ) {
//        digitalWrite( outRotexReset, RESTVALUE );   
//  }
//}
