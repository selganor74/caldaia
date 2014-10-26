int ledPin[6] = {
  5,6,10,11,12,13};
int inPin[6] = {
  2,3,4,7,8,9};
int analogValue[4] = {
  0,0,0,0};
int var[6] = {
  0,0,0,0,0,0};

unsigned int vettore[6] = {
  0,0,0,0,0};

boolean Connection;
byte i = 0;
char val;

void setup() {
  for (i = 0; i < 6; i++) {
    pinMode(ledPin[i], OUTPUT);
    digitalWrite(ledPin[i], LOW);  /* Inizializzo l'uscita a 0. */
  }

  for (i = 0; i < 6; i++) {
    pinMode(inPin[i], INPUT);
  }

  Serial.begin(9600);
  Connection = false;
}

void loop(){
            if (Serial.available() && !Connection) {
                                    val = Serial.read();
                                    if (val =='*'){
                                                    delay(100);
                                                    Serial.print('C');
                                                    delay(100);
                                                    Connection = true;

                                                   }else
                                                   {
                                                    delay(100);
                                                    Serial.print('N');
                                                    delay(100);
                                                    Connection = false;

                                                   }                                                   
                                   }
                                   
            if (Connection == true){

                            if (Serial.available()){
                                                    val = Serial.read();
                                                    if (val =='O'){
                                                                    delay(100);
                                                                    Serial.print('d');
                                                                    delay(100);

                                                                    for (i = 0; i < 6; i++) {
                                                                                            while (Serial.available() == 0) ;
                                                                                            vettore[i] = Serial.read();
                                                                                            Serial.print(vettore[i]-48);                                                                                            
                                                                                            if((vettore[i]-48) == 1){
                                                                                                                    digitalWrite(ledPin[i], HIGH);
                                                                                                                    }else
                                                                                                                    {
                                                                                                                    digitalWrite(ledPin[i], LOW);                                                                                                                      
                                                                                                                    }
                                                                                            }

                                                                    
                                                    Serial.print('E');
                                                    Serial.print(',');
                                                    for (i = 0; i < 6; i++) {
                                                                               var[i] = digitalRead(inPin[i]);  /* Leggo lo stato degli ingressi. */
                                                                            }
                                                    Serial.print("D_IN");
                                                    Serial.print(',');
                                                    for (i = 0; i < 6; i++) {
                                                                            Serial.print(var[i]+48, BYTE);
                                                                            Serial.print(',');
                                                                            }                        
                                                    Serial.print(',');
                                                    
                                                    for (i = 0; i < 4; i++) {
                                                                            analogValue[i] = analogRead(i);
                                                                            Serial.print("AN_");
                                                                            Serial.print(i+1, DEC);
                                                                            Serial.print(",");
                                                                            Serial.print(analogValue[i]);
                                                                            Serial.print(',');
                                                                            delay(10);
                                                                            }

                                                    //Serial.println();
                                                    
                                                    Serial.print(128, BYTE);                
                                                    Connection = false; 

                                                   }else
                                                   {
                                                    delay(100);
                                                    Serial.print('X');
                                                    delay(100);
                                                    Connection = false;
                                                    digitalWrite(ledPin[0], LOW);                                                    
                                                   }                                                   
                                                 }
                                                                                                 
                                  }                       
}

