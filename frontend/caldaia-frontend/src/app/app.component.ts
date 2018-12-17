import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { BackendService } from './backend.service';

import { IDataFromArduino } from './idata-from-arduino';
import { ISettingsFromArduino } from './isettings-from-arduino';

import { SignalRIntegration } from './signalr-integration';

import { environment } from '../environments/environment';

declare var $: any; // JQueryStatic;
@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
  providers: [BackendService]
})
export class AppComponent implements OnInit {

  private signalrBaseUrl = environment.signalrBaseUrl;
  public data: IDataFromArduino = {};
  public settings: ISettingsFromArduino = {};
  public signalrStatus: string;
  public showDisconnectedDialog = false;

  constructor(
    private cdr: ChangeDetectorRef,
    private _backend: BackendService
  ) { }

  ngOnInit(): void {
    this.configSignalR();
  }

  private configSignalR() {
    const _hubConnection = $.hubConnection(this.signalrBaseUrl);

    new SignalRIntegration(_hubConnection)
      .onStateChanged(this.onSignalRStatusChange.bind(this))
      .onDataReceived(this.onDataReceived.bind(this))
      .onSettingsReceived(this.onSettingsReceived.bind(this))
      .start();
  }

  private onDataReceived(data: IDataFromArduino) {
    this.data = data;
    this.cdr.detectChanges();
  }

  private onSettingsReceived(settings: ISettingsFromArduino) {
    this.settings = settings;
    this.cdr.detectChanges();
  }

  private onSignalRStatusChange(state: number, stateDescription: string) {
    if (state !== 1) { this.showDisconnectedDialog = true; }
    if (state === 1) {
      this.showDisconnectedDialog = false;
      this.refreshSettings();
    }
    this.signalrStatus = stateDescription;
    this.cdr.detectChanges();
  }

  public refreshData() {
    this._backend.updateLatestData().subscribe();
  }

  public refreshSettings() {
    this._backend.updateLatestSettings().subscribe();
  }

  public incrementMaxTempConCamino() {
    this._backend.incrementMaxTempConCamino();
  }

  public decrementMaxTempConCamino() {
    this._backend.decrementMaxTempConCamino();
  }

  public incrementMinTempConCamino() {
    this._backend.incrementMinTempConCamino();
  }

  public decrementMinTempConCamino() {
    this._backend.decrementMinTempConCamino();
  }

  public incrementTempSamplingInterval() {
    this._backend.incrementTempSamplingInterval();
  }

  public decrementTempSamplingInterval() {
    this._backend.decrementTempSamplingInterval();
  }

  public incrementTIsteresiCaldaia() {
    this._backend.incrementTIsteresiCaldaia();
  }

  public decrementTIsteresiCaldaia() {
    this._backend.decrementTIsteresiCaldaia();
  }
}
