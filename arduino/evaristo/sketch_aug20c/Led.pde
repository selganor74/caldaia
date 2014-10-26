class Led{
  int x,y;
  int size;
  color baseGray;
  color onGray;
  
  boolean on = false;
  
  Led(int xp, int yp, int s, color b, color o){
    x = xp;
    y = yp;
    size = s;
    baseGray = b;
    onGray = o;
  }
  
  boolean active(){
    on = true;
    return true;
  }
  
   boolean notactive(){
    on = false;
    return false;
  }
  
  void display(){
    if (on == true){
      fill(onGray);
    }else{
      fill (baseGray);
    }
    stroke(255);
    rect(x, y, size, size);
  }
  
}
