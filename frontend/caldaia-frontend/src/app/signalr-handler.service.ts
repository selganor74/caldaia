import { Injectable } from '@angular/core';
import { SignalRIntegration } from './signalr-integration';
import { environment } from '../environments/environment';
import { IDataFromArduino } from './idata-from-arduino';
import { ISettingsFromArduino } from './isettings-from-arduino';

declare let $: any;

@Injectable({
  providedIn: 'root'
})
export class SignalrHandlerService {

  private _signalrIntegration: SignalRIntegration;

  constructor(
  ) {
    if (!environment.signalrBaseUrl) { throw new Error('environment.signalrBaseUrl must be set !!!'); }
    this.configSignalR();
  }

  private configSignalR() {

    const _hubConnection = $.hubConnection(environment.signalrBaseUrl);
    this._signalrIntegration = new SignalRIntegration(_hubConnection)
      .start();
  }

  public start() {
    this._signalrIntegration.start();
  }

  registerHandlerForOnDataFromArduino(handler: (data: IDataFromArduino) => void) {
    this._signalrIntegration.onDataReceived(handler);
  }

  registerHandlerForOnSettingsFromArduino(handler: (data: ISettingsFromArduino) => void) {
    this._signalrIntegration.onSettingsReceived(handler);
  }

  registerHandlerForOnStateChanged(handler: (state: number, stateDescription: string) => void) {
    this._signalrIntegration.onStateChanged(handler);
  }

  registerHandlerForOnRaw(handler: (raw: string) => void) {
    this._signalrIntegration.onRawReceived(handler);
  }

  unregisterHandler(handler: Function) {
    this._signalrIntegration.unregisterHandler(handler);
  }
}
