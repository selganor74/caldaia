#ifndef HWSETUP_H
#define HWSETUP_H

// const int outEthReset = 12;  // M19 Pin per reset EthShield - Sembrerebbe utilizzato dall'ethernet shield
// vedi: http://www.arduino.cc/cgi-bin/yabb2/YaBB.pl?num=1286370597/8

// PIN SETUP
/*
J3    Arduino       Evaristo
8 AREF    -
7 GND   -
6 13 (SCK)  Out-6 (M18) 
5 12 (MIS0) Out-5 (M19) outPompaCamino. Comanda la pompa del camino.
4 11 (MOSI) Out-4 (M20) outRotexReset. Invia impulsi di 5 secondi ad un
                                    rele NC per spegnere e riaccendere la centralina ROTEX.
                                    [2017 01 17 - Sostituito dall'override del Termostato Ambiente]
                                    outOverrideTermoAmbiente [Normalmente Aperto]
3 10 (SS)   Out-3 (M21) TX_Seriale Rotex <<-- Sebbene non sia usato, viene comunque occupato dalla libreria seriale !
2 9   In-6  (M6)
1 8   In-5  (M5)

                        In-   (M9) ain ingresso temperatura camino tramite NTC. 

J1
8 7   In-4  (M4) (TP9) 
7 6   Out-2 (M22) Relay Pompa
6 5   Out-1 (M23) Relay Caldaia
5 4   In-3  (M3) RX_Seriale Rotex
4 3   In-2  (M2) Termostato Ambiente
3 2   In-1  (M1) Termostato Accumulatore 
2 1 (TXD)
1 0 (RXD)
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



void ioSetup();

#endif
