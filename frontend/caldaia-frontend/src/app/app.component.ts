import { Component, OnInit, Output, Input, ChangeDetectorRef } from '@angular/core';
import { BackendService } from './backend.service';
import { Message } from 'primeng/api';

import {IDataFromArduino} from './idata-from-arduino';
import { ISettingsFromArduino } from './isettings-from-arduino';

import { environment } from '../environments/environment';

declare var $: any; // JQueryStatic;
@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
  providers: [BackendService]
})
export class AppComponent implements OnInit {

  private _hubConnection: any; // SignalR.Hub.Connection;
  private dataProxy: any; // SignalR.Hub.Proxy;
  private settingsProxy: any; // SignalR.Hub.Proxy;

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
    this.startSignalR();
  }

  private configSignalR() {
    this._hubConnection = $.hubConnection(this.signalrBaseUrl);
    this._hubConnection.reconnectDelay = 2000;
    this._hubConnection.stateChanged((state) => {
      const stateConversion = {0: 'connecting', 1: 'connected', 2: 'reconnecting', 4: 'disconnected'};
      this.signalrStatus = stateConversion[state.newState];
      console.log('SignalR: Connection status changed', this.signalrStatus);

      if (state.newState !== 1) { this.showDisconnectedDialog = true; }
      if (state.newState === 1) { this.showDisconnectedDialog = false; }
      this.cdr.detectChanges();
    });

    this.dataProxy = this._hubConnection.createHubProxy('data');
    this.settingsProxy = this._hubConnection.createHubProxy('settings');

    this.dataProxy.on('notify', (payload: any) => {
      console.log('SignalR: Received data on dataProxy', payload);
      this.data = payload;
      this.cdr.detectChanges();
    });

    this.settingsProxy.on('notify', (payload: any) => {
      console.log('SignalR: Received data on settingsProxy', payload);
      this.settings = payload;
      this.cdr.detectChanges();
    });

    this._hubConnection
      .disconnected(() => {
        // when disconnected we should display a message and block any user request
       console.log('SignalR Connection lost! Trying to reconnect');
       this.startSignalR();
      });
  }

  private startSignalR() {

    this._hubConnection
      .start()
      .done(() => {
        console.log('SignalR: Connection started!');
        this.refreshSettings();
      })
      .catch(err => {
        console.log('SignalR: Error while establishing connection :(', err);
      });

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
