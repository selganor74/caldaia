import processing.serial.*;
import cc.arduino.*;

Arduino arduino;

Scrollbar bar1, bar2, bar3;
Indicator ind1, ind2, ind3, ind4;
Button   button1_on, button1_off,
         button2_on, button2_off,
         button3_on, button3_off;
Led led1_b, led2_b, led3_b,
    led1, led2, led3, led4, led5, led6;

PFont font;

boolean out1 = false;
boolean out2 = false;
boolean out3 = false;

int analog0, analog1, analog2, analog3;

int[] values = { 
  Arduino.LOW, Arduino.LOW, Arduino.LOW, Arduino.LOW,
  Arduino.LOW, Arduino.LOW, Arduino.LOW, Arduino.LOW, Arduino.LOW,
  Arduino.LOW, Arduino.LOW, Arduino.LOW, Arduino.LOW, Arduino.LOW
};

void setup(){
  size(600,600);
  noStroke();
  disegnatoreInterfaccia();

  arduino = new Arduino(this,"COM5",57600);

  for (int i = 2; i <= 4; i++) arduino.pinMode(i,Arduino.INPUT);
  for (int i = 7; i <= 9; i++) arduino.pinMode(i,Arduino.INPUT);
  for (int i = 5; i <= 6; i++) arduino.pinMode(i,Arduino.OUTPUT);
  for (int i = 10; i <= 13; i++) arduino.pinMode(i,Arduino.OUTPUT);
  
  analog0 = 0;
  analog1 = 0;
  analog2 = 0;
  analog3 = 0;
 
  
}

void draw(){
  background(204);
  fill(0);
  
    text("PWM_1=", 150, 70);
    int pos1 = int(bar1.getPos());
    text(nf(pos1, 2), 200, 70);
    text("%", 220, 70);
    float pwm1_arduino = pos1*255/100;
    arduino.analogWrite(5, int(pwm1_arduino)); 
    
    text("PWM_2=", 150, 100);
    int pos2 = int(bar2.getPos());
    text(nf(pos2, 2), 200, 100);
    text("%", 220, 100);  
    float pwm2_arduino = pos2*255/100;
    arduino.analogWrite(6, int(pwm2_arduino));     
    
    text("PWM_3=", 150, 130);
    int pos3 = int(bar3.getPos());
    text(nf(pos3, 2), 200, 130);
    text("%", 220, 130);     
    float pwm3_arduino = pos3*255/100;
    arduino.analogWrite(10, int(pwm3_arduino)); 
    
    text("Analog 0 - punti=", 220, 160);
    analog0 = arduino.analogRead(0);
    text(nf(analog0,2), 350, 160);
    
    
    text("Analog 1 - punti=", 220, 190);
    analog1 = arduino.analogRead(1);
    text(nf(analog1,2), 350, 190);    
    
    text("Analog 2 - punti=", 220, 220);
    analog2 = arduino.analogRead(2);
    text(nf(analog2,2), 350, 220);   
    
    text("Analog 3 - punti=", 220, 250);
    analog3 = arduino.analogRead(3);
    text(nf(analog3,2), 350, 250);
    
    text("Out_1", 145, 280);
    text("Out_2", 145, 310);
    text("Out_3", 145, 340);
    
    text("Input_1", 155, 370);
    if (arduino.digitalRead(2) == Arduino.HIGH){
      led1.active();
      }else{led1.notactive();
      } 
    
    text("Input_2", 155, 400);
    if (arduino.digitalRead(3) == Arduino.HIGH){
      led2.active();
      }else{led2.notactive();
      }  
      
    text("Input_3", 155, 430);
    if (arduino.digitalRead(4) == Arduino.HIGH){
      led3.active();
      }else{led3.notactive();
      } 
      
    text("Input_4", 155, 460);
    if (arduino.digitalRead(7) == Arduino.HIGH){
      led4.active();
      }else{led4.notactive();
      } 
      
    text("Input_5", 155, 490);
    if (arduino.digitalRead(8) == Arduino.HIGH){
      led5.active();
      }else{led5.notactive();
      } 
      
    text("Input_6", 155, 520);
    if (arduino.digitalRead(9) == Arduino.HIGH){
      led6.active();
      }else{led6.notactive();
      } 
      
  if (out1){
            led1_b.active();
            arduino.digitalWrite(11, Arduino.HIGH);
  }else{
        led1_b.notactive();
        arduino.digitalWrite(11, Arduino.LOW);        
  }
  
  if (out2){
            led2_b.active();
            arduino.digitalWrite(12, Arduino.HIGH);
  }else{
        led2_b.notactive();
        arduino.digitalWrite(12, Arduino.LOW);
  }
  
  if (out3){
            led3_b.active();
            arduino.digitalWrite(13, Arduino.HIGH);
  }else{
        led3_b.notactive();
        arduino.digitalWrite(13, Arduino.LOW);
  }
  
  manageInterface();
}



