import processing.serial.*;

Serial myPort;  // Create object from Serial class
int[] button = new int[6];
int[] led = new int[6];
int[] bar = new int[4];
int syncStatus, cont;
int val;

boolean ConnectionEnstab1 = false;
String inBuffer;
String[] q;

void setup(){
  
String portName ="COM5";
myPort = new Serial(this, portName, 9600);  
    
fill(30);
smooth();
size(640,1037);

for (int i=0; i < 6; i=i+1){              //inizializzazione vettore button
                            button[i] = 0;
                            }
                            
for (int i=0; i < 6; i=i+1){              //inizializzazione vettore led
                            led[i] = 0;
                            }
for (int i=0; i < 4; i=i+1){              //inizializzazione vettore bar
                            bar[i] = 0;
                            }                            

syncStatus = 0;
cont = 0;
                            
led[4] = 0;
led[0] = 0;

bar[0] = 30;
bar[1] = 96;
bar[2] = 90;
bar[3] = 12;
                            
strokeWeight(1);
strokeJoin(ROUND);

fill(220,220,220);
rect(10,10,620,140);   //AREA OUT
rect(10,160,620,140);  //AREA IN
rect(10,310,620,200);  //AREA ANALOG
rect(10,520,620,80);  //AREA SYNC

PFont font;
font = loadFont("zigurrat-50.vlw");
textSize(30);
fill(0);

text("output", 500, 80);
text("input", 500, 240);
text("5 V", 50, 340);
text("0", 70, 480);
text("Analog", 500, 400);
text("input", 500, 440);
text("Evaristo 01 - demo software", 15, 640);
text("COM5", 15, 680);

}

void draw(){


//STATO OUT++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
for (int i=0; i < 6; i=i+1){
                            
                            fill(60*button[i],130*button[i],240*button[i]);
                            rect(20+i*80,40,40,40);
                            text(1+i, 30+i*80, 120);
                            
                            //illuminazione pulsante, se mouse sorvola---------------------------------------------                          
                            if (overRect(20+i*80,40,40,40)){
                                                            strokeWeight(3);
                                                            stroke(240, 231, 60);
                                                            fill(60*button[i],130*button[i],240*button[i]);
                                                            rect(20+i*80,40,40,40);
                                                           }
                                                           else
                                                           {
                                                            strokeWeight(3); 
                                                            stroke(0, 0, 0); 
                                                            fill(60*button[i],130*button[i],240*button[i]); 
                                                            rect(20+i*80,40,40,40);                                                           
                                                           }
                           //--------------------------------------------------------------------------------------                            

                           //cambio stato del pulsante se tasto mouse premuto *************************************
                            if ((overRect(20+i*80,40,40,40)) && mousePressed)
                                                                             {
                                                                              delay(200); 
                                                                              if (button[i] == 0)
                                                                                                  {
                                                                                                   button[i] = 1;
                                                                                                  } 
                                                                                                  else
                                                                                                  {
                                                                                                   button[i] = 0;
                                                                                                  }
                                                                             }
                           //***************************************************************************************
                           }
//END STATO OUT+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

//STATO IN IIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIII
for (int i=0; i < 6; i=i+1){
                            strokeWeight(3); 
                            stroke(0, 0, 0);
                            fill(241*led[i],250*led[i],18*led[i]);
                            rect(20+i*80,200,40,40);
                            text(1+i, 30+i*80, 280);
                           }

//END STATO IN IIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIII

//STATO ANALOG AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
for (int i=0; i < 4; i=i+1){
                            strokeWeight(3); 
                            stroke(0, 0, 0);
                            fill(9,106,16);
                            rect(130+i*100,340,40,128);
                            text(1+i, 140+i*100, 500);
                              
                                                        fill(0);
                                                        rect(130+i*100,340,40,128-bar[i]);
                           }

//STATO ANALOG AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA

//STATO SYNC SSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSS
                            
                            fill(60*syncStatus,130*syncStatus,240*syncStatus);
                            rect(140,540,40,40);
                            text("Sync", 30, 580);
                            
                            //illuminazione pulsante, se mouse sorvola---------------------------------------------                          
                            if (overRect(140,540,40,40)){
                                                            strokeWeight(3);
                                                            stroke(240, 231, 60);
                                                            fill(60*syncStatus,130*syncStatus,240*syncStatus);
                                                            rect(140,540,40,40);
                                                            }
                                                           else
                                                           {
                                                            strokeWeight(3); 
                                                            stroke(0, 0, 0); 
                                                            fill(60*syncStatus,130*syncStatus,240*syncStatus);
                                                            rect(140,540,40,40);                                                           
                                                           }
                           //--------------------------------------------------------------------------------------
                           //cambio stato del pulsante se tasto mouse premuto *************************************
                            if ((overRect(140,540,40,40)) && mousePressed){   
                                                                              delay(200); 
                                                                              if (syncStatus == 0)
                                                                                                  {
                                                                                                   syncStatus = 1;
                                                                                                  } 
                                                                                                  else
                                                                                                  {
                                                                                                   syncStatus = 0;
                                                                                                  }

                                                      

                                                                             }
                           //*************************************************************************************** 
//END STATO SYNC SSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSS

if (syncStatus == 1){
                    cont += 1;
                    delay(100);
                    if (cont == 5){

                                    myPort.write('*');                                                          
                                    syncStatus =0;
                                    cont = 0;
                                    ConnectionEnstab1 = true;
                                    delay(200);
                                    myPort.write('O');
                                    delay(200);
                                    for (int i=0; i < 6; i=i+1){
                                                                myPort.write(button[i]+48);
                                                               }
                                    
                                   }
                    }
                    
  while (myPort.available() > 0) {
                                  inBuffer = myPort.readStringUntil(128);
                                                                     //myPort.clear();
                                  //q = splitTokens(inBuffer, ", ");                                   

                                  if (inBuffer != null) {
                                                         println(inBuffer);
                                                         q = splitTokens(inBuffer, ",");
                                                        
                                                         for (int i=0; i < 6; i=i+1){
                                                                                    println(q[2+i].charAt(0));
                                                                                    led[i] = q[2+i].charAt(0)-48;                                                                                    
                                                                                    }
                                                         for (int i=0; i <4 ; i=i+1){
                                                                                     println(q[9+i*2]);
                                                                                    }
                                                         for (int i=0; i <4 ; i=i+1){
                                                                                    bar[i] = ((q[9+i*2].charAt(0)-48)*100 + (q[9+i*2].charAt(1)-48)*10 + (q[9+i*2].charAt(2)-48))/8;
                                                                                    println(bar[i]);
                                                                                   }                          
                                                        
                                                      }
                                                        
                               }                         
                                                       
if (ConnectionEnstab1 = true){    
                                   
                                //println(q[1]);
                               //ConnectionEnstab1 = false; 
                              }                              
                    
/*
if (myPort.available()>0) { // If data is available to read,
                         val = myPort.read(); // read it and store it in val
                         myPort.clear();                         
                         if ((val == 67) && !ConnectionEnstab1){
                                                                ConnectionEnstab1 = true;
                                                                myPort.write('O');                                                                
                                                                }
                                                               else{
                                                                   ConnectionEnstab1 = false;
                                                                   }                       
                        }
                        
if ((myPort.available()>0) && ConnectionEnstab1) {
                                                 val = myPort.read();
                                                 myPort.clear();                         
                                                 if (val == 100){
                                                                for (int i=0; i < 6; i=i+1){
                                                                                            myPort.write(button[i]+48);
                                                                                           }             
                                                                }
                         }
                         */

}



boolean overRect(int x, int y, int w, int h) 
{
  if (mouseX >= x && mouseX <= x+w && 
      mouseY >= y && mouseY <= y+h) {
    return true;
  } else {
    return false;
  }
}
