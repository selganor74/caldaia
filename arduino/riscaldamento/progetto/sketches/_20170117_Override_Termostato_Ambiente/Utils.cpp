#include <SPI.h>
#include "Utils.h"

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
