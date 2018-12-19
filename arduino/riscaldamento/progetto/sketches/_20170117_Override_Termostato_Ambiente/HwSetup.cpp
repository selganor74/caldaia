#include <SPI.h>

#include "HwSetup.h"


void ioSetup() {
  pinMode( outPompaCamino, OUTPUT );
  pinMode( outPompa, OUTPUT );
  pinMode( outCaldaia, OUTPUT );
  pinMode( inTermoAmbiente, INPUT );
  // pinMode( outRotexReset, OUTPUT );
  pinMode( outOverrideTermoAmbiente, OUTPUT );
  pinMode( inTermoAccumulatore, INPUT );
  // pinMode( ainTempCamino, INPUT );
}
