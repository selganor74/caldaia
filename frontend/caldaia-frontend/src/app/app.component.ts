import { Component, OnInit, Output, Input } from '@angular/core';
import { BackendService } from './backend.service';
import { Message } from 'primeng/api';

import {IDataFromArduino} from './idata-from-arduino';
import { ISettingsFromArduino } from './isettings-from-arduino';

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

  @Output() data: IDataFromArduino = {};
  @Input() settings: ISettingsFromArduino = {};
  @Output() msgs: Message[] = [];

  constructor(private _backend: BackendService) { }

  ngOnInit(): void {
    this._hubConnection = $.hubConnection('http://localhost:32767/signalr');
    this.dataProxy = this._hubConnection.createHubProxy('data');
    this.settingsProxy = this._hubConnection.createHubProxy('settings');

    this._hubConnection
      .start()
      .done(() => console.log('Connection started!'))
      .catch(err => console.log('Error while establishing connection :('));

    this.dataProxy.on('notify', (payload: any) => {
      console.log('SignalR: Received data on dataProxy', payload);
      this.data = payload;
    });

    this.settingsProxy.on('notify', (payload: any) => {
      console.log('SignalR: Received data on settingsProxy', payload);
      this.settings = payload;
    });
  }

  public refreshData() {
    this._backend.updateLatestData().subscribe();
  }

  public refreshSettings() {
    this._backend.updateLatestSettings().subscribe();
  }

}
