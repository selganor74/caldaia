// In questa versione mancano:
//    Data Logging. Nessuna Seriale, Niente Ethernet
//    Temperatura dell'accumulatore (non) letta tramite termostato

const int outPompa = 6;      // M22 Relay Pompa. M22
const int outCaldaia = 5;    // M23 Relay Caldaia. M23
const int pinV3vApri = 10;   // M21 Relay Valvola 3 Vie Apri    M21
const int pinV3vChiudi = 11; // M20 Relay Valvola 3 Vie Chiudi M20

const int inTermoAmbiente = 2;     // M1 Input Termostati Ambiente
const int inTermoAccumulatore = 3; // M2 Input Termostato Accumulatore

void setup() {
  pinMode( outPompa, OUTPUT );
  pinMode( outCaldaia, OUTPUT );
  pinMode( inTermoAmbiente, INPUT );
  pinMode( inTermoAccumulatore, INPUT );
  pinMode( pinV3vApri, OUTPUT );
  pinMode( pinV3vChiudi, OUTPUT );
  // Per motivi di Debug.
  Serial.begin(9600);
}

void loop() {
  // Rallentiamo il ciclo a una volta ogni 500 ms
  delay(500);
  // Controllo Relay Pompa
  if ( digitalRead( inTermoAmbiente ) == HIGH ) {
    digitalWrite( outPompa, HIGH );
  } else {
    digitalWrite( outPompa, LOW );
  }

  // Controllo Relay Accumulatore
  if ( digitalRead( inTermoAccumulatore ) == HIGH ) {
    digitalWrite( outCaldaia, HIGH );
  } else {
    digitalWrite( outCaldaia, LOW );
  }
}
