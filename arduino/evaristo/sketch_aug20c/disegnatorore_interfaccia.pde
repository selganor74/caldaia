void disegnatoreInterfaccia(){
  
  color gray = color(204);
  color white = color(255);
  color black = color(0);
  
  font = loadFont("Courier-20.vlw");
  textFont(font);
  textAlign(CENTER);
  
  bar1 = new Scrollbar(10, 60, 100, 10, 0.0, 100.0);
  bar2 = new Scrollbar(10, 90, 100, 10, 0.0, 100.0);
  bar3 = new Scrollbar(10, 120, 100, 10, 0.0, 100.0);
  
  ind1 = new Indicator(10, 150, 100, 10);
  ind2 = new Indicator(10, 180, 100, 10);
  ind3 = new Indicator(10, 210, 100, 10);
  ind4 = new Indicator(10, 240, 100, 10);
  
  button1_on = new    Button(10, 270, 10, gray, white, black);
  button1_off = new   Button(30, 270, 10, gray, white, black); 
  led1_b = new Led(50, 270, 10, black, white);
  
  button2_on = new    Button(10, 300, 10, gray, white, black);
  button2_off = new   Button(30, 300, 10, gray, white, black); 
  led2_b = new Led(50, 300, 10, black, white);
  
  button3_on = new    Button(10, 330, 10, gray, white, black);
  button3_off = new   Button(30, 330, 10, gray, white, black); 
  led3_b = new Led(50, 330, 10, black, white);
  
  led1 = new Led(10, 360, 10, black, white);
  led2 = new Led(10, 390, 10, black, white);
  led3 = new Led(10, 420, 10, black, white);
  led4 = new Led(10, 450, 10, black, white);
  led5 = new Led(10, 480, 10, black, white);  
  led6 = new Led(10, 510, 10, black, white);
}
