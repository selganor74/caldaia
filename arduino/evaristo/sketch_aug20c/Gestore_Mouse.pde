void mousePressed(){
  bar1.press(mouseX, mouseY);
  bar2.press(mouseX, mouseY);
  bar3.press(mouseX, mouseY);
  
  if (button1_on.press() == true){out1 = true;}
  if (button1_off.press() == true){out1 = false;}

  if (button2_on.press() == true){out2 = true;}
  if (button2_off.press() == true){out2 = false;}

  if (button3_on.press() == true){out3 = true;}
  if (button3_off.press() == true){out3 = false;}
}


void mouseReleased(){
  
  bar1.release();
  bar2.release();
  bar3.release();

  button1_on.release();
  button1_off.release();

  button2_on.release();
  button2_off.release();
  
  button3_on.release();
  button3_off.release();
  
}
