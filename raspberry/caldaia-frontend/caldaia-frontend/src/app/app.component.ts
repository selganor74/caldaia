import { Component } from '@angular/core';
import { IDataFromArduino } from './idata-from-arduino';
import { SignalrAdapterService } from './signalr-adapter.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styles: []
})
export class AppComponent {
  title = 'caldaia-frontend';

  public tAccumulo: number = 0;
  public tCamino: number = 0;
  public tPannelli: number = 0;
  public rPompaCamino: boolean = false;
  public rPompaRiscaldamento: boolean = false;
  public rBypassTermostatoAmbiente: boolean = false;
  public rCaldaia: boolean = false;
  public pompaPannelli: boolean = false;
  public termoAmbienti: boolean = false;
  public termoRotex: boolean = false;

  constructor(
    private adapter: SignalrAdapterService
  ) {
    adapter.onDataReceived(d => this.onDataReceived(d));
  }

  private onDataReceived(d: IDataFromArduino) {
    if (!d)
      return;

    this.tAccumulo = d.rotexTS || 0;
    this.tCamino = Math.round((d.ainTempCaminoValueCentigradi || 0) * 10) / 10 ;
    this.tPannelli = d.rotexTK || 0;
    this.rPompaCamino = d.outPompaCaminoValue == 1;
    this.rPompaRiscaldamento = d.outPompaValue == 1;
    this.rBypassTermostatoAmbiente = d.outOverrideTermoAmbienteValue == 1;
    this.rCaldaia = d.outCaldaiaValue == 1;
    this.pompaPannelli = d.rotexP1 == 1;
    this.termoAmbienti = d.inTermoAmbienteValue == 1;
    this.termoRotex = d.inTermoAccumulatoreValue == 1;
  }
}
