
const int pinPompa = 6;
const int pinTermo = 5;
const int pinV3vApri = 10;
const int pinV3vChiudi = 12;

const int pinBtnPompa = 2;
const int pinBtnTermo = 3;

void setup() {
  pinMode( pinPompa, OUTPUT );
  pinMode( pinTermo, OUTPUT );
  pinMode( pinBtnPompa, INPUT );
  pinMode( pinBtnTermo, INPUT );
  pinMode( pinV3vApri, OUTPUT );
  pinMode( pinV3vChiudi, OUTPUT );
}

void loop() {
  if ( digitalRead( pinBtnPompa ) == HIGH ) {
    digitalWrite( pinPompa, HIGH );
    digitalWrite( pinV3vApri, HIGH );
  } else {
    digitalWrite( pinPompa, LOW );
    digitalWrite( pinV3vApri, LOW );
  }

  if ( digitalRead( pinBtnTermo ) == HIGH ) {
    digitalWrite( pinTermo, HIGH );
    digitalWrite( pinV3vChiudi, LOW );
  } else {
    digitalWrite( pinTermo, LOW );
    digitalWrite( pinV3vChiudi, LOW );
  }

}
