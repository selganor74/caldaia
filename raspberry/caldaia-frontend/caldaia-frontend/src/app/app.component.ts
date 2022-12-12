import { ChangeDetectorRef, Component } from '@angular/core';
import { IDataFromArduino } from './idata-from-arduino';
import { SignalrAdapterService } from './signalr-adapter.service';
import { Measure, State } from "./caldaia-state";

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styles: []
})
export class AppComponent {
  title = 'caldaia-frontend';

  public tAccumulo: string = "";
  public tCamino: string = "";
  public tPannelli: string = "";
  public rPompaCamino: string = "";
  public rPompaRiscaldamento: string = "";
  public rBypassTermostatoAmbiente: string = "";
  public rCaldaia: string = "";
  public pompaPannelli: string = "";
  public termoAmbienti: string = "";
  public termoRotex: string = "";

  public consoleOut: string = "";
  public oldConsoleOut: string = "";

  public state: Measure[] = [];
  drawing: boolean = false;

  constructor(
    private cdr: ChangeDetectorRef,
    adapter: SignalrAdapterService
  ) {
    adapter.onDataReceived(d => this.onDataReceived(d));
    adapter.onCaldaiaStateReceived(d => this.onCaldaiaStateReceived(d));
  }

  onCaldaiaStateReceived(d: any): void {
    // if (this.drawing)
    //   return;
    // this.drawing = true;
    
    const q = <State>d;
    this.state = [];
    for (const i in q) {
      q[i].name = i;
      this.state.push(q[i]);
    }

    this.tAccumulo = q["roteX_TEMP_ACCUMULO"]?.formattedValue ?? this.tAccumulo;
    this.tCamino = q["temperaturA_CAMINO"]?.formattedValue ?? this.tCamino;
    this.tPannelli = q["roteX_TEMP_PANNELLI"]?.formattedValue ?? this.tPannelli;
    this.rPompaCamino = q["statO_RELAY_POMPA_CAMINO"]?.formattedValue ?? this.rPompaCamino;
    this.rPompaRiscaldamento = q["statO_RELAY_POMPA_RISCALDAMENTO"]?.formattedValue ?? this.rPompaRiscaldamento;
    this.rBypassTermostatoAmbiente = q["statO_RELAY_BYPASS_TERMOSTATO_AMBIENTE"]?.formattedValue ?? this.rBypassTermostatoAmbiente;
    this.rCaldaia = q["statO_RELAY_CALDAIA"]?.formattedValue ?? this.rCaldaia;
    this.pompaPannelli = q["roteX_STATO_POMPA"]?.formattedValue ?? this.pompaPannelli;
    this.termoAmbienti = q["termostatO_AMBIENTI"]?.formattedValue ?? this.termoAmbienti;
    this.termoRotex = q["termostatO_ROTEX"]?.formattedValue ?? this.termoRotex;


    let c = 
` Temperatura Accumulo : ${this.tAccumulo}             
   Temperatura Camino : ${this.tCamino}                   
 Temperatura Pannelli : ${this.tPannelli}               
  Termostato Ambienti : ${this.termoAmbienti}    
     Termostato Rotex : ${this.termoRotex}
         Pompa Camino : ${this.rPompaCamino}
       Pompa Pannelli : ${this.pompaPannelli}
Bypass Termo Ambienti : ${this.termoAmbienti}
`;
    this.consoleOut = c;
    this.cdr.markForCheck();
    // let start = 0;
    // let current = 0;
    // let end = Math.max(c.length, this.consoleOut.length);
    // this.oldConsoleOut = this.consoleOut;
    // let cancel = setInterval(() => {
    //   current++;
    //   if (current <= end) {
    //     let crLf = this.oldConsoleOut.substring(current,current+1) == "\n" ? "\n" : "";
    //     this.consoleOut = c.substring(start, current) + "_" + crLf + this.oldConsoleOut.substring(current+1);
    //   }
    //   if (current > end) {
    //     clearInterval(cancel);
    //     this.drawing = false;
    //     this.consoleOut = c;
    //   }        
    //   //this.cdr.markForCheck();
    // }, 2);
  }

  private onDataReceived(d: IDataFromArduino) {
    if (!d)
      return;

    // this.tAccumulo = d.rotexTS || 0;
    // this.tCamino = Math.round((d.ainTempCaminoValueCentigradi || 0) * 10) / 10;
    // this.tPannelli = d.rotexTK || 0;
    // this.rPompaCamino = d.outPompaCaminoValue == 1;
    // this.rPompaRiscaldamento = d.outPompaValue == 1;
    // this.rBypassTermostatoAmbiente = d.outOverrideTermoAmbienteValue == 1;
    // this.rCaldaia = d.outCaldaiaValue == 1;
    // this.pompaPannelli = d.rotexP1 == 1;
    // this.termoAmbienti = d.inTermoAmbienteValue == 1;
    // this.termoRotex = d.inTermoAccumulatoreValue == 1;
  }
}
