#ifndef SETTINGS_H
#define SETTINGS_H

struct Settings {
    char header[5] = "CSV1";
    char rotexTermoMin = 43;      // Temperatura di accensione delle caldaia
    char rotexTermoMax = 45;      // Temperatura di spegnimento della caldaia
    char deltaSolare   = 1;       // Quando i pannelli sono in funzione la temperatura di soglia della caldaia (rotexTermoMin) scende di deltaSolare Gradi.
    char rotexMaxTempConCamino = 71; // Se la temperatura dell'accumulo rotex è maggiore o uguale a rotexMaxTempConCamino viene attivato l'override 
                                       // del termostato ambiente mandando in circolo in impianto.
    char rotexMinTempConCamino = 69; // L'override viene sganciato quando la temperatura dell'accumulo scende sotto rotexMinTempConCamino.

    // Tempo minimo (in millisecondi) di accensione della caldaia
    unsigned long T_ISTERESI_CALDAIA = 1200000 ;
    // Millisecondi tra un'acquisizione temperature e l'altra.
    unsigned long TEMP_SAMPLING_INTERVAL = 12000 ;  
} currentSettings;

void settingsToSerial();

bool loadSettings(); 

void saveSettings();

#endif
