#ifndef ROTEX_H
#define ROTEX_H

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

#define ROTEX_MAX_STRING_LEN 36

// Variabile usate dal sistema Rotex
struct RotexStatus {
  public:
  static unsigned long rotexHasFailed;
  static int rotexValues[12];
  static float rotexPortataTV;
  static unsigned long rotexLastRead;

  static char rotexLastReadString[ROTEX_MAX_STRING_LEN];
};

void rotexSerialSetup();

void rotexReadSerial();

//void doRotexReset();

#endif
