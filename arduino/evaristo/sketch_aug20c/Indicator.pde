class Indicator{
  int x,y;         // coordinate x e y
  int sw, sh;    // larghezza e altezza
  float ratio;       // posizione dell'indicatore
  float pos;
  
  Indicator(int xp, int yp, int w, int h){
    x = xp;
    y = yp;
    sw = w;
    sh = h; 
    pos = xp + int(w * 0.2);
  }
  
  void update(float rat){
      pos = x + int(sw * rat / (100+(sh/2)));
  }
    
  void display(){
    fill(255);
    rect(x, y, sw, sh);
    fill(0);
    
    rect(int(pos), y, sh, sh);
  }
  
}
