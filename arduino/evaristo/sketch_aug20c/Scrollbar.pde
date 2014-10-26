class Scrollbar{
  int x,y;         // coordinate x e y dello scrollbar
  float sw, sh;    // larghezza e altezza dello scrollbar
  float pos;       // posizione dell'indicatore
  float posMin, posMax;  // posizione minima e massima dell'indicatore
  boolean rollover;     // vero quando il mouse è sopra 
  boolean locked;       // vero quando lo scrollbar è attivo
  float minVal, maxVal; //valore minimo e massimo dell'indicatore
  
  Scrollbar(int xp, int yp, int w, int h, float miv, float mav){
    x = xp;
    y = yp;
    sw = w;
    sh = h;
    minVal = miv;
    maxVal = mav;
    pos = x + sw/2 - sh/2;
    posMin = x;
    posMax = x + sw - sh;    
  }
  
  void update(int mx, int my){
    if (over(mx, my) == true){
      rollover = true;
    }else{
      rollover = false;
     }
    if (locked == true){
      pos = constrain(mx-sh/2, posMin, posMax);
    }  
  }
  
  void press(int mx, int my){
    if (rollover == true){
      locked = true;
    }else{
      locked = false;
     }    
  }
  
  void release(){
    locked = false;
  }
  
  boolean over(int mx, int my){
    if ((mx > x) && (mx < x+sw) && (my > y) && (my < y+sh)){
      return true;
    }else{
      return false;
    }
  }
  
  void display(){
    fill(255);
    rect(x, y, sw, sh);
    if ((rollover == true) || (locked == true)){
      fill(0);
    }else{
      fill(102);
    }
    rect(pos, y, sh, sh);
  }
  
  float getPos(){
    float scalar = sw/(sw - sh);
    float ratio = (pos - x) * scalar;
    float offset = minVal + (ratio * (maxVal - minVal) / sw);
    return offset;
  }
  
}
