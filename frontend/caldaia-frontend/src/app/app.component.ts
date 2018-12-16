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

  @Output() data: IDataFromArduino = {};
  @Input()  settings: ISettingsFromArduino = {};
  @Output() msgs: Message[] = [];

  constructor(
    private cdr: ChangeDetectorRef,
    private _backend: BackendService
    ) { }

  ngOnInit(): void {
    this._hubConnection = $.hubConnection(this.signalrBaseUrl);
    this._hubConnection.reconnectDelay = 2000;
    this.dataProxy = this._hubConnection.createHubProxy('data');
    this.settingsProxy = this._hubConnection.createHubProxy('settings');

    this._hubConnection
      .start()
      .done(() => {
        console.log('Connection started!');
        this.refreshSettings();
      })
      .catch(err => console.log('Error while establishing connection :(', err));

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
