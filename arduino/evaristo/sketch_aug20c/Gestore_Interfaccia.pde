void manageInterface(){  
  
  bar1.update(mouseX, mouseY);
  bar1.display(); 
  
  bar2.update(mouseX, mouseY);
  bar2.display(); 
  
  bar3.update(mouseX, mouseY);
  bar3.display();  
  
  float analog0_ratio = analog0*100/1024;
  ind1.update(analog0_ratio); 
  ind1.display();

  float analog1_ratio = analog1*100/1024;
  ind2.update(analog1_ratio);    
  ind2.display();

  float analog2_ratio = analog2*100/1024;
  ind3.update(analog2_ratio);    
  ind3.display();

  float analog3_ratio = analog3*100/1024;
  ind4.update(analog3_ratio); 
  ind4.display();  
  
  button1_on.update(); 
  button1_on.display();
  button1_off.update();  
  button1_off.display();
  led1_b.display();  

  button2_on.update();    
  button2_on.display();
  button2_off.update();    
  button2_off.display();   
  led2_b.display();  
  
  button3_on.update();  
  button3_on.display();
  button3_off.update();    
  button3_off.display(); 
  led3_b.display();  
  
  led1.display();  
  led2.display();  
  led3.display();  
  led4.display();  
  led5.display();   
  led6.display();  
  
  
}



