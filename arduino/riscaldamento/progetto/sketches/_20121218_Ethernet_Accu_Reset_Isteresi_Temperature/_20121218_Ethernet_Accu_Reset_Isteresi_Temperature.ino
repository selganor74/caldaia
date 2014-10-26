#include <SPI.h>
#include <Client.h>
#include <Ethernet.h>
#include <Server.h>
#include <Udp.h>

// Numero di campioni memorizzati nel buffer circolare
#define  CB_MAX_VALUES  60
#define  CB_AVG_REBUILD_AFTER 1
#include <CircularBuffer.h>

boolean serialDebug;

CircularBuffer cbTempAmbienteCentigradi = CircularBuffer();

// In questa versione:
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
//    Data Logging. Nessuna Seriale
//    Temperatura dell'accumulatore (non) letta tramite termostato
// const int outEthReset = 12;  // M19 Pin per reset EthShield - Sembrerebbe utilizzato dall'ethernet shield
// vedi: http://www.arduino.cc/cgi-bin/yabb2/YaBB.pl?num=1286370597/8
const int outPompa = 5;      // M22 Relay Pompa. M22
const int outCaldaia = 6;    // M23 Relay Caldaia. M23
// Utilizzando lo shield ETHERNET i pin 11, 12, 13 vengono utilizzati per gestire la 
// scheda ethernet stessa quindi non devono essere utilizzati.
// const int pinV3vApri = 10;   // M21 Relay Valvola 3 Vie Apri    M21
// const int pinV3vChiudi = 11; // M20 Relay Valvola 3 Vie Chiudi M20

const int inTermoAmbiente = 3;     // M1 Input Termostati Ambiente
const int inTermoAccumulatore = 2; // M2 Input Termostato Accumulatore
const int ainTempAmbiente = 0 ;    // M9 Input analogico LM35 Temperatura ambiente

// Millisecondi di isteresi prima che la caldaia possa spegnersi 1h=60*60*1000=3600000  20min = 1200000
const unsigned long T_ISTERESI_CALDAIA = 1200000 ;
// Millisecondi tra un'acquisizione temperature e l'altra.
const unsigned long TEMP_SAMPLING_INTERVAL = 10000 ;
// Millis dell'ultima acquisizione temperature
unsigned long lastTempAcquired;
// Costanti per l'output 
#define ACCESO "1"
#define SPENTO "0"

// Costanti per Ethernet
byte mac[]={ 
  0xF1, 0xCA, 0xAB, 0xBA, 0x77, 0xEE };
byte ip[] = { 
  192, 168, 2, 40 };    
byte gateway[] = { 
  192, 168, 2, 1 };
byte subnet[] = { 
  255, 255, 255, 0 };

// Inizializzazione del Server.
EthernetServer server = EthernetServer(80);

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

unsigned long ethResetCounter; // Contatore per i reset dell'ethernet shield. 
                               // Viene azzerato insieme agli accumulatori

unsigned long isteLastOutCaldaia_On;
unsigned long isteLastOutCaldaia_On_For;

int   ainTempAmbienteValue;
float ainTempAmbienteValueCentigradi;

void setup() {
  // Utilizziamo la seriale per fare un po' di debug.
  Serial.begin(9600);
  serialDebug = false;
  
  ioSetup();

  initVariables();

  resetAccumulators();

  ethernetSetup();
  
  // imposto il riferimeno analogico agli 1.1 volt interni
  analogReference( INTERNAL );
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
  // gestione della ethernet
  doEthernet();
/*  
  if (serialDebug ) {
    Serial.print( "loopLengthMillis: " );
    Serial.println( loopLengthMillis );
    loopLengthMillis = calcolaIntervallo( loopStartMillis, millis() );
  }
*/
}

void resetAccumulators() {
  outPompaAccu_On=0;
  outPompaAccu_Off=0;
  outCaldaiaAccu_On=0;
  outCaldaiaAccu_Off=0;
  inTermoAmbienteAccu_On=0;
  inTermoAmbienteAccu_Off=0;
  inTermoAccumulatoreAccu_On=0;
  inTermoAccumulatoreAccu_Off=0;
  
  ethResetCounter = 0;
}

void initVariables() {
  outPompaValue=false;

  outCaldaiaValue=false;

  inTermoAmbienteValue=false;

  inTermoAccumulatoreValue=false;

  ainTempAmbienteValue=0;
  ainTempAmbienteValueCentigradi=0.0;
  lastTempAcquired = 0;
}

void ioSetup() {
  pinMode( outPompa, OUTPUT );
  pinMode( outCaldaia, OUTPUT );
  pinMode( inTermoAmbiente, INPUT );
  pinMode( inTermoAccumulatore, INPUT );
  // pinMode( ainTempAmbiente, INPUT );
}

void ethernetSetup() {

  Ethernet.begin(mac, ip, gateway, subnet);

  server.begin();
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
}

// Elaborazione dei valori letti
void doCrunchInputs() {
  // Elaborazione degli output
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
  inTermoAmbienteValue     == HIGH ? outPompaValue   = HIGH : outPompaValue = LOW; 
  // inTermoAccumulatoreValue == HIGH ? outCaldaiaValue = HIGH : outCaldaiaValue = LOW;   
}

unsigned long calcolaIntervallo( unsigned long da, unsigned long a ) {
  return a >= da ? a - da : a + (0-1-da);
  
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

}

// Impostazione dell'output
void doSetOutputs() {
  digitalWrite( outPompa,   outPompaValue   );
  digitalWrite( outCaldaia, outCaldaiaValue );  
}

void doEthernet() {
  EthernetClient client = server.available();
  char lastChar = 0x00;
  char thisChar = 0x00;
  static int crlfCount = 0;
  static long lastEthRequestTime = 0;
  long timeSinceLastEthRequest = 0;
  unsigned long startTime;
  unsigned long endTime;
  unsigned long timeToProcess;

  const char resetString[]="resetAccu*";
  static int resetIndex=0;
  static boolean shouldResetAccu = false;
  if (client) {
    startTime = millis();
    while ( client.available() ) {
      thisChar = client.read();
      if (thisChar == resetString[resetIndex]) resetIndex++; 
      else resetIndex=0;
      if (resetString[resetIndex]=='*') { 
        shouldResetAccu=true;
        Serial.println("Should Reset Accumulators");
        resetIndex=0;
      }

      // Serial.print( thisChar, DEC );
      // Serial.print( " = " );
      // Serial.println( byte( thisChar ) );      
      if( lastChar == 13 && thisChar == 10 ) {
        crlfCount++;
        // Serial.println( crlfCount ); 
      } 
      else {
        if( thisChar != 13 )
          crlfCount = 0;
      }
      if( crlfCount >= 2) {
        crlfCount=0;
        // Abbiamo ricevuto un doppio invio... rispondiamo!
        ethernetPrintResponseHeader( &client );

        ethernetPrintContentHeader( &client );

        // output the value of each analog input pin
        ethernetPrintValue( "outPompaValue",            outPompaValue            == HIGH ? ACCESO : SPENTO, &client );
        ethernetPrintValue( "outCaldaiaValue",          outCaldaiaValue          == HIGH ? ACCESO : SPENTO, &client );
        ethernetPrintValue( "inTermoAmbienteValue",     inTermoAmbienteValue     == HIGH ? ACCESO : SPENTO, &client );
        ethernetPrintValue( "inTermoAccumulatoreValue", inTermoAccumulatoreValue == HIGH ? ACCESO : SPENTO, &client );
        
        ethernetPrintValueULong( "ainTempAmbienteValue", ainTempAmbienteValue, &client );
        ethernetPrintValueFloat( "ainTempAmbienteValueCentigradi", ainTempAmbienteValueCentigradi, &client );
        ethernetPrintValueFloat( "cbTempAmbienteCentigradiAvg", cbTempAmbienteCentigradi.current_avg, &client );
        ethernetPrintValueFloat( "cbTempAmbienteCentigradiMin", cbTempAmbienteCentigradi.current_min, &client );
        ethernetPrintValueFloat( "cbTempAmbienteCentigradiMax", cbTempAmbienteCentigradi.current_max, &client );
        
        ethernetPrintValueULong( "inTermoAccumulatoreAccu_On",  inTermoAccumulatoreAccu_On,  &client ); 
        ethernetPrintValueULong( "inTermoAccumulatoreAccu_Off", inTermoAccumulatoreAccu_Off, &client );
        ethernetPrintValueULong( "inTermoAmbienteAccu_On",      inTermoAmbienteAccu_On,      &client );
        ethernetPrintValueULong( "inTermoAmbienteAccu_Off",     inTermoAmbienteAccu_Off,     &client );
        ethernetPrintValueULong( "outCaldaiaAccu_On",           outCaldaiaAccu_On,           &client );
        ethernetPrintValueULong( "outCaldaiaAccu_Off",          outCaldaiaAccu_Off,          &client );
        ethernetPrintValueULong( "outPompaAccu_On",             outPompaAccu_On,             &client );
        ethernetPrintValueULong( "outPompaAccu_Off",            outPompaAccu_Off,            &client );
        ethernetPrintValueULong( "isteLastOutCaldaia_On",       isteLastOutCaldaia_On,       &client );
        ethernetPrintValueULong( "isteLastOutCaldaia_On_For",   isteLastOutCaldaia_On_For,   &client );
        ethernetPrintValueULong( "T_ISTERESI_CALDAIA",          T_ISTERESI_CALDAIA,          &client );
        ethernetPrintValueULong( "TEMP_SAMPLING_INTERVAL",      TEMP_SAMPLING_INTERVAL,      &client );
        ethernetPrintValueULong( "lastTempAcquired",            lastTempAcquired,            &client );
        ethernetPrintValueULong( "millis",       millis(),       &client );
        if (ethResetCounter != 0) {
          ethernetPrintValueULong( "ethResetCounter",             ethResetCounter,             &client );
        }

        ethernetPrintContentFooter( &client );

        // give the web browser time to receive the data
        delay(5);
        // close the connection:
        client.stop();
        lastEthRequestTime = millis();
        timeSinceLastEthRequest = millis() - lastEthRequestTime;
        if (serialDebug) {
          Serial.print("lastEthRequestTime: ");
          Serial.println(lastEthRequestTime);
        }
        if (shouldResetAccu) {
          resetAccumulators();
          shouldResetAccu=false;
        }
        break;
      }
      lastChar = thisChar;
    }
    endTime = millis();
    timeToProcess = endTime - startTime;
    if ( serialDebug ) {
      Serial.print("Ethernet Processing Time: ");
      Serial.println(timeToProcess);
    }
    // Giusto nel caso di un reset manuale della scheda, reimposto
    // Indirizzo Ip e riavvio la libreria Ethernet.

  }
  timeSinceLastEthRequest = millis() - lastEthRequestTime;
  if (timeSinceLastEthRequest > 120000) { // Se sono passati più di due minuti dall'ultima richiesta, reimposto la scheda eth
    if ( serialDebug ) {
      Serial.println("120 secs since last Eth Request.");
      Serial.println("Reinitializing Ethernet Shield");
    }
    ethResetCounter++;
    ethernetSetup();
    lastEthRequestTime = millis();
  } 
}

void ethernetPrintResponseHeader( Client *client ) {
  client->println("HTTP/1.1 200 OK");
  client->println("Content-Type: text/xml");
  client->println();
}

void ethernetPrintContentHeader( Client *client ) {
  client->println("<?xml version=\"1.0\" ?>");
  client->println("<data id=\"arduino\">");
}

void ethernetPrintContentFooter( Client *client ) {
  client->println("</data>");
}

void ethernetPrintValue( const char *valueName, const char *value, Client *client ) {
  client->print( "<value name=\"" );
  client->print( valueName );
  client->print( "\" value=\"" );
  client->print( value );
  client->println( "\" />" );
}

void ethernetPrintValueULong( const char *valueName, const unsigned long value, Client *client ) {
  client->print( "<value name=\"" );
  client->print( valueName );
  client->print( "\" value=\"" );
  client->print( value );
  client->println( "\" />" );
}

void ethernetPrintValueFloat( const char *valueName, const float value, Client *client ) {
  static char tmp[15];
  dtostrf( value, 1, 2, tmp );
  client->print( "<value name=\"" );
  client->print( valueName );
  client->print( "\" value=\"" );
  client->print( tmp );
  client->println( "\" />" );
}

