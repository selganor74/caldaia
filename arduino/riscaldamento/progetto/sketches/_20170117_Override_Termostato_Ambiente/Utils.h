#ifndef UTILS_H
#define UTILS_H

unsigned long calcolaIntervallo( unsigned long da, unsigned long a );

// Genera un impulso di lunghezza "milliseconds"
void pulse( unsigned long outputPin, unsigned long milliseconds );

void blinkOutput( unsigned long outputId, unsigned long blinkInterval );

#endif
