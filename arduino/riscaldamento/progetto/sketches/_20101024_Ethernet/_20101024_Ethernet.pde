#include <SPI.h>
#include <Client.h>
#include <Ethernet.h>
#include <Server.h>
#include <Udp.h>

// In questa versione:
//   Possibilità d interrogare arduino via ethernet
//   per avere lo stato dei sensori e degli attuatori.

// In questa versione mancano:
//    Data Logging. Nessuna Seriale
//    Temperatura dell'accumulatore (non) letta tramite termostato

const int outPompa = 5;      // M22 Relay Pompa. M22
const int outCaldaia = 6;    // M23 Relay Caldaia. M23
// Utilizzando lo shield ETHERNET i pin 11, 12, 13 vengono utilizzati per gestire la 
// scheda ethernet stessa quindi non devono essere utilizzati.
// const int pinV3vApri = 10;   // M21 Relay Valvola 3 Vie Apri    M21
// const int pinV3vChiudi = 11; // M20 Relay Valvola 3 Vie Chiudi M20

const int inTermoAmbiente = 3;     // M1 Input Termostati Ambiente
const int inTermoAccumulatore = 2; // M2 Input Termostato Accumulatore

// Costanti per l'output 
#define ACCESO "1"
#define SPENTO "0"

// Costanti per Ethernet
byte mac[]={ 0xF1, 0xCA, 0xAB, 0xBA, 0x77, 0xEE };
byte ip[] = { 192, 168, 2, 40 };    
byte gateway[] = { 192, 168, 2, 1 };
byte subnet[] = { 255, 255, 255, 0 };

// Inizializzazione del Server.
Server server = Server(80);

boolean outPompaValue;
boolean outCaldaiaValue;
boolean inTermoAmbienteValue;
boolean inTermoAccumulatoreValue;

void setup() {
  ioSetup();

  ethernetSetup();
  
  // Utilizziamo la seriale per fare un po' di debug.
  Serial.begin(9600);
}

void loop() {
  // lettura delle variabili.
  doReadInputs();
  // elaborazione degli ingressi
  doCrunchInputs();
  // impostazione degli output
  doSetOutputs();
  
  // gestione della ethernet
  doEthernet();

}

void ioSetup() {
  pinMode( outPompa, OUTPUT );
  pinMode( outCaldaia, OUTPUT );
  pinMode( inTermoAmbiente, INPUT );
  pinMode( inTermoAccumulatore, INPUT );
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
}

// Elaborazione dei valori letti
void doCrunchInputs() {
  inTermoAmbienteValue     == HIGH ? outPompaValue   = HIGH : outPompaValue = LOW; 
  inTermoAccumulatoreValue == HIGH ? outCaldaiaValue = HIGH : outCaldaiaValue = LOW; 
}

// Impostazione dell'output
void doSetOutputs() {
  digitalWrite( outPompa,   outPompaValue   );
  digitalWrite( outCaldaia, outCaldaiaValue );  
}

void doEthernet() {
  Client client = server.available();
  char lastChar = 0x00;
  char thisChar = 0x00;
  static int crlfCount = 0;
  unsigned long startTime;
  unsigned long endTime;
  unsigned long timeToProcess;
  if (client) {
    startTime = millis();
    while ( client.available() ) {
      thisChar = client.read();
      // Serial.print( thisChar, DEC );
      // Serial.print( " = " );
      // Serial.println( byte( thisChar ) );      
      if( lastChar == 13 && thisChar == 10 ) {
        crlfCount++;
        // Serial.println( crlfCount ); 
      } else {
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
        
        ethernetPrintContentFooter( &client );

        // give the web browser time to receive the data
        delay(1);
        // close the connection:
        client.stop();

        break;
      }
      lastChar = thisChar;
    }
    endTime = millis();
    timeToProcess = endTime - startTime;
    Serial.print("Ethernet Processing Time: ");
    Serial.println(timeToProcess);
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


