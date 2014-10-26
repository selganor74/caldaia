int current;

void setup() {
  int i;
  for(int i = 0; i < 15; i++ ) {
    pinMode( i, OUTPUT ); 
  }
  current = 0;
}

void loop() {
  digitalWrite(current, HIGH);
  delay(1000);
  digitalWrite(current, LOW );
  current < 15 ? current ++ : current = 0;
 
}
