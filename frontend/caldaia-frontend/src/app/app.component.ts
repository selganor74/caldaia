import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { BackendService } from './backend.service';

import { IDataFromArduino } from './idata-from-arduino';
import { ISettingsFromArduino } from './isettings-from-arduino';

import { SignalrHandlerService } from './signalr-handler.service';

import { environment } from '../environments/environment';

declare var $: any; // JQueryStatic;
@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
  providers: [BackendService, SignalrHandlerService]
})
export class AppComponent implements OnInit {

  private signalrBaseUrl = environment.signalrBaseUrl;
  public data: IDataFromArduino = {};
  public settings: ISettingsFromArduino = {};
  public signalrStatus: string;
  public showDisconnectedDialog = false;

  public isTerminalVisible = false;
  public selectedTab;

  constructor(
    private cdr: ChangeDetectorRef,
    private _backend: BackendService,
    private _signalrHandler: SignalrHandlerService
  ) { }

  ngOnInit(): void {
    this.configSignalR();
  }

  public onTabViewChange(e: any) {
    this.selectedTab = e;
  }

  private configSignalR() {
    this._signalrHandler.registerHandlerForOnStateChanged(this.onSignalRStatusChange.bind(this));
    this._signalrHandler.registerHandlerForOnDataFromArduino(this.onDataReceived.bind(this));
    this._signalrHandler.registerHandlerForOnSettingsFromArduino(this.onSettingsReceived.bind(this));

    this._signalrHandler.start();
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
    this._backend.updateLatestData();
  }

  public refreshSettings() {
    this._backend.updateLatestSettings();
  }

  public saveSettings() {
    this._backend.saveSettings();
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


  public incrementRotexTermoMax() {
    console.log('incrementRotexTermoMax');
    this._backend.incrementRotexTermoMax();
  }

  public decrementRotexTermoMax() {
    console.log('decrementRotexTermoMax');
    this._backend.decrementRotexTermoMax();
  }

  public incrementRotexTermoMin() {
    console.log('incrementRotexTermoMin');
    this._backend.incrementRotexTermoMin();
  }

  public decrementRotexTermoMin() {
    console.log('decrementRotexTermoMin');
    this._backend.decrementRotexTermoMin();
  }

  public pausePoller() {
    console.log('pausePoller for 12 seconds');
    this._backend.pausePoller(12);
  }
}
