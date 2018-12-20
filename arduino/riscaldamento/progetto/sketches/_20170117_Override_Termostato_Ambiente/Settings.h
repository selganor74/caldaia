#ifndef SETTINGS_H
#define SETTINGS_H

#include <SPI.h>

static const char HEADER[5] = "CSV3";

struct Settings {

    char header[5] = { HEADER[0], HEADER[1], HEADER[2], HEADER[3], HEADER[4] } ;
    char rotexTermoMin = 43;      // Temperatura di accensione delle caldaia
    char rotexTermoMax = 45;      // Temperatura di spegnimento della caldaia
    char rotexMaxTempConCamino = 71; // Se la temperatura dell'accumulo rotex è maggiore o uguale a rotexMaxTempConCamino viene attivato l'override 
                                       // del termostato ambiente mandando in circolo in impianto.
    char rotexMinTempConCamino = 69; // L'override viene sganciato quando la temperatura dell'accumulo scende sotto rotexMinTempConCamino.

    // Tempo minimo (in millisecondi) di accensione della caldaia
    unsigned long T_ISTERESI_CALDAIA = 1200000 ;
    // Millisecondi tra un'acquisizione temperature e l'altra.
    unsigned long TEMP_SAMPLING_INTERVAL = 12000 ;  

    char deltaTInnescoPompaCamino = 3;   // Quando il delta tra rotexTS e la temperatura del camino supera questa temperatura, la pompa del camino parte.
    // OR
    char TCaminoPerAccensionePompa = 62; // Se la temperatura del camino è superiore a questa temperatura allora la pompa parte

    char TInnescoSeRotexNonDisponibile = 55; // Se l'accumulo ROTEX non è disponibile, allora la pompa del camino parte quando la temperatura del camino supera TInnescoSeRotexNonisponibile
    char TDisinnescoSeRotexNonDisponibile = 53; // Se il Rotex non è disponibile la pompa del camino si deve spegnere sotto questa soglia.

    char TInnescoOverrideTermostatoSeRotexNonDisponibile = 65;      // Se ROTEX non è disponibile, l'override del riscaldamento partirà se la temperatura del Camino supera i 65*
    char TDisinnescoOverrideTermostatoSeRotexNonDisponibile = 60;   // Se ROTEX non è disponibile, l'override del riscaldamento si fermerà se la temperatura del Camino scende sotto ai *
    
    char TDisinnescoOverrideSeRotexDisponibile  = 60;               // Se ROTEX è disponibile e la temperatura scende sotto questa soglia, allora l'override del riscaldamento può essere staccato.
};

void settingsToSerial(Settings settings);

Settings loadSettingsFromEEPROM(); 

Settings getDefaultSettings();

void saveSettingsToEEPROM(Settings settings);

#endif
