#include <SPI.h>

// #include <Time.h>

#include <Dhcp.h>
#include <Dns.h>
#include <Ethernet.h>
#include <EthernetClient.h>
#include <EthernetServer.h>
#include <EthernetUdp.h>
#include <util.h>

#include <SoftwareSerial.h>




// Numero di campioni memorizzati nel buffer circolare
#define  CB_MAX_VALUES  5
#define  CB_AVG_REBUILD_AFTER 1
#include <CircularBuffer.h>

// NTP Stuff
// ntp.ien.it : 193.204.114.105
/*
IPAddress timeServer(193, 204, 114, 105 ); //NTP server address
unsigned int localPort=8888; //local port to listen for UDP packets
const int NTP_PACKET_SIZE=48; //NTP time stamp
byte packetBuffer[NTP_PACKET_SIZE]; //buffer to hold incoming and outgoing packets
const unsigned long timeZoneOffset=3600; //set this to the offset in seconds to your local time;

EthernetUDP Udp;
//*/
// Abilita o disabilita il debug tramite seriale
boolean serialDebug;

CircularBuffer cbTempAmbienteCentigradi;
CircularBuffer cbRotexTS;
//CircularBuffer cbRotexTR;
CircularBuffer cbRotexTK;
//CircularBuffer cbRotexTV;
CircularBuffer cbRotexPortataTV;

// In questa versione:
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
6	13 (SCK)	Out-6 (M18) xEthernet Shield
5	12 (MIS0)	Out-5 (M19) xEthernet Shield
4	11 (MOSI)	Out-4 (M20) xEthernet Shield
3	10 (SS)		Out-3 (M21) TX_Seriale Rotex
2	9		In-6  (M6)
1	8		In-5  (M5)

J1
8	7		In-4  (M4)
7	6		Out-2 (M22) Relay Pompa
6	5		Out-1 (M23) Relay Caldaia
5	4		In-3  (M3) RX_Seriale Rotex
4	3		In-2  (M2) Termo Ambiente
3	2		In-1  (M1) Termo Accumulatore
2	1 (TXD)
1	0 (RXD)	
*/
// Utilizzando lo shield ETHERNET i pin 11, 12, 13 vengono utilizzati per gestire la 
// scheda ethernet stessa quindi non devono essere utilizzati.

const int outPompa = 5;            // M22 Relay Pompa. M22
const int outCaldaia = 6;          // M23 Relay Caldaia. M23
const int serialRotexRX = 4;       // M3 -> Pin 4  -> Evaristo In-3 -> M3
const int serialRotexTX = 10;      // M21-> Pin 10 -> Evaristo In-3 -> M21 -- NON E' CONNESSO --
const int inTermoAmbiente = 3;     // M1 Input Termostati Ambiente
const int inTermoAccumulatore = 2; // M2 Input Termostato Accumulatore
const int ainTempAmbiente = 0 ;    // M9 Input analogico LM35 Temperatura ambiente

SoftwareSerial rotexSerial = SoftwareSerial(4,10); //Initialize 2nd serial port (rx,tx)

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

// Variabile usate dal sistema Rotex
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
unsigned long avgNumberOfReads;

int   ainTempAmbienteValue;
float ainTempAmbienteValueCentigradi;

void setup() {
  // Utilizziamo la seriale per fare un po' di debug.
  Serial.begin(9600);
  rotexSerial.begin(9600);
  serialDebug = false;
  
  ioSetup();

  initVariables();

  resetAccumulators();

  ethernetSetup();
  
  // timeSetup();
  
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
  rotexLastRead = 0;
  rotexPortataTV = 0.0;
}

/*
void timeSetup() {
  Udp.begin(localPort);
  int i;
  i = 0;
  setSyncProvider(getNtpTime);
  setSyncInterval(86400);
  Serial.print("Synching Clock...");
  while(timeStatus()==timeNotSet && i < 10 ){
    //Serial.print("."); //wait until the time is set by the sync provider
    delay(2000);
    i++;
  }
  if (timeStatus()==timeNotSet) {
    Serial.println("Unable to Synch Clock");
  } else {
    Serial.println("Clock in Synch:");
    TimeElements tm;  
    breakTime( now(), tm );
    Serial.print( tm.Year );
    Serial.print( " " );
    Serial.print( tm.Month );
    Serial.print( " " );
    Serial.print( tm.Day );
    Serial.print( " " );
    Serial.print( tm.Hour );
    Serial.print( " " );
    Serial.print( tm.Minute );
    Serial.print( " " );
    Serial.print( tm.Second );
    Serial.println( " " );
  }
}
//*/

void ioSetup() {
  pinMode( outPompa, OUTPUT );
  pinMode( outCaldaia, OUTPUT );
  pinMode( inTermoAmbiente, INPUT );
  pinMode( inTermoAccumulatore, INPUT );
  // pinMode( ainTempAmbiente, INPUT );
}

void ethernetSetup() {

  Ethernet.begin(mac, ip, gateway, subnet);
  // timeSetup();
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
  // letturaRotex
  doReadRotex(); // Imposta anche rotexLastRead
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


// Appena accessa la centralina R3 restituisce questa stringa:
//            SOLARIS R3 V.3.1 May 30 2007 14:37:58 ROTEX GmbH Ser.-Nr: 000001******
// Successivamente:
// L'output seriale della R3 è un record con i valori separati da ';' 
//            HA;BK;P1 /%;P2;TK /Ã¸C;TR /Ã¸C;TS /Ã¸C;TV /Ã¸C;;V /l/min
boolean doReadRotex() {
  boolean tmp_return;
  int i;
  String currVal;
  unsigned char buf[10];
  char inchar;
  boolean foundSemicolon;
  i = 0;
  tmp_return = false;
  currVal = "";
  foundSemicolon = false;
  while ( rotexSerial.available() > 0 ) {
    inchar = rotexSerial.read();
    inchar == ',' ? inchar = '.' : inchar = inchar;
    if ( inchar == ';' || ( ( inchar == 13 ) && ( foundSemicolon == true ) ) ) {
      foundSemicolon = true;
      currVal.getBytes( &buf[0], 10 );
      /*
      Serial.print ( " --- " );
      Serial.print ( i );
      Serial.print (" String: " );
      Serial.print ( (char *)&buf[0] );
      Serial.print (" Number: " );
      //*/
      if( i == 8 ) { // se i == 8 cioè per la portata
        rotexPortataTV = atof( (char *)&buf[0] );
        //Serial.println (rotexPortataTV);
      } else { 
        rotexValues[ i ] = atoi( (char *)&buf[0] );
        //Serial.println (rotexValues[i]);
      }
      Serial.print( currVal );
      Serial.print( inchar );
      currVal = "";
      i++;
    } else {
      currVal = currVal + inchar;
      //Serial.print( inchar );
    }
  }
  // la lettura è valida se il numero di ";" è esattamente 11 (compreso il chr13 finale)
  if ( i == 11 ) {
    tmp_return = true;
    rotexLastRead = millis();
    cbRotexTS.addValue( rotexValues[RTX_TS]);
    //cbRotexTR.addValue( rotexValues[RTX_TR]);
    cbRotexTK.addValue( rotexValues[RTX_TK]);
    //cbRotexTV.addValue( rotexValues[RTX_TV]);
    cbRotexPortataTV.addValue( rotexPortataTV );  
  } else {
    tmp_return = false;
    if (i != 0 ) rotexLastRead = 0;
  }
  if ( i != 0 ) {
    Serial.print( " (" );
    Serial.print( i );
    Serial.println( ")" );
  }
  return tmp_return;
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
        if(serialDebug) {
          Serial.println("Should Reset Accumulators");
        }
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

        // Sezione Rotex
        /*
        if (rotexLastRead != 0) {
          ethernetPrintValueULong( "rotexLastRead",  rotexLastRead,        &client );          
          ethernetPrintValueULong( "rotexHA",        rotexValues[RTX_HA],  &client );          
          ethernetPrintValueULong( "rotexBK",        rotexValues[RTX_BK],  &client );          
          ethernetPrintValueULong( "rotexP1",        rotexValues[RTX_P1],  &client );          
          ethernetPrintValueULong( "rotexP2",        rotexValues[RTX_P2],  &client );          
          ethernetPrintValueULong( "rotexTK",        rotexValues[RTX_TK],  &client );          
          ethernetPrintValueULong( "rotexTR",        rotexValues[RTX_TR],  &client );          
          ethernetPrintValueULong( "rotexTS",        rotexValues[RTX_TS],  &client );          
          ethernetPrintValueULong( "rotexTV",        rotexValues[RTX_TV],  &client );          
          ethernetPrintValueULong( "rotexQT",        rotexValues[RTX_QT],  &client );          
          
          ethernetPrintValueFloat( "rotexPortataTV",  rotexPortataTV,  &client );    
          // rotexLastRead = 0;      
        }
        //*/
        if (cbRotexTS.total_values != 0) {
          ethernetPrintValueFloat( "cbRotexTS", cbRotexTS.current_avg, &client );
          //ethernetPrintValueFloat( "cbRotexTR", cbRotexTR.current_avg, &client );
          ethernetPrintValueFloat( "cbRotexTK", cbRotexTK.current_avg, &client );
          //ethernetPrintValueFloat( "cbRotexTV", cbRotexTV.current_avg, &client );
          ethernetPrintValueFloat( "cbRotexPortataTV", cbRotexPortataTV.current_avg, &client );
        }
        
        ethernetPrintValueULong( "millis",       millis(),       &client );
        if (ethResetCounter != 0) {
          ethernetPrintValueULong( "ethResetCounter",             ethResetCounter,             &client );
        }

        ethernetPrintContentFooter( &client );

        // give the web browser time to receive the data
        delay(15);
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

/* NTP Functions */
/*******************************************************************************
* Get NTP time function
*******************************************************************************/
/*
unsigned long getNtpTime(){
  sendNTPpacket(timeServer); //send an NTP packet to a time server
  delay(2000); //wait to see if a reply is available
  if ( Udp.available() ){
    Udp.read(packetBuffer,NTP_PACKET_SIZE); //read the packet into the buffer

    //the timestamp starts at byte 40 of the received packet and is four bytes,
    //or two words, long. First, esxtract the two words:
    unsigned long highWord = word(packetBuffer[40], packetBuffer[41]);
    unsigned long lowWord = word(packetBuffer[42], packetBuffer[43]);
    //combine the four bytes (two words) into a long integer
    //this is NTP time (seconds since Jan 1 1900):
    unsigned long secsSince1900 = highWord << 16 | lowWord;
    const unsigned long seventyYears = 2208988800UL - timeZoneOffset;
    //subtract seventy years:
    return secsSince1900 - seventyYears;
  }
  return 0; //return 0 if unable to get the time
}
// send an NTP request to the time server at the given address
unsigned long sendNTPpacket(IPAddress& address)
{
  // set all bytes in the buffer to 0
  memset(packetBuffer, 0, NTP_PACKET_SIZE);
  // Initialize values needed to form NTP request
  // (see URL above for details on the packets)
  packetBuffer[0] = 0b11100011;   // LI, Version, Mode
  packetBuffer[1] = 0;     // Stratum, or type of clock
  packetBuffer[2] = 6;     // Polling Interval
  packetBuffer[3] = 0xEC;  // Peer Clock Precision
  // 8 bytes of zero for Root Delay & Root Dispersion
  packetBuffer[12]  = 49;
  packetBuffer[13]  = 0x4E;
  packetBuffer[14]  = 49;
  packetBuffer[15]  = 52;

  // all NTP fields have been given values, now
  // you can send a packet requesting a timestamp:         
  Udp.beginPacket(address, 123); //NTP requests are to port 123
  Udp.write(packetBuffer,NTP_PACKET_SIZE);
  Udp.endPacket();
}
//*/
