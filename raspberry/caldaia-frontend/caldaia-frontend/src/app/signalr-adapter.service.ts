import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { IDataFromArduino } from './idata-from-arduino';

@Injectable({
  providedIn: 'root'
})
export class SignalrAdapterService {

  private _dataReceivedHandlers: ((data: IDataFromArduino) => void)[] = [];
  private _hubConnection: HubConnection // SignalR.Hub.Connection;


  constructor() {
    this._hubConnection = this.configSignalR("/datahub");
    this.start();
  }

  public unregisterHandler(handler: Function) {
    const handlersArray: Array<Array<Function>> = [
      this._dataReceivedHandlers
    ];

    for (const ha of handlersArray) {
      const toRemove = ha.indexOf(handler);
      if (toRemove !== -1) {
        console.log('unregistering handler', handler);
        ha.splice(toRemove, 1);
      }
    }
  }

  public onDataReceived(handler: (data: IDataFromArduino) => void) {
    if (handler) {
      this._dataReceivedHandlers.push(handler);
    }
    return this;
  }

  private fireDataReceived(data: IDataFromArduino) {
    for (const handler of this._dataReceivedHandlers) {
      try {
        console.info("received data", data);
        handler(data);
      } catch (err) {
        console.error('fireDataReceived: Errors while executing handler', handler.toString());
      }
    }
  }

  private configSignalR(signalRBaseUrl: string): HubConnection {

    this._hubConnection = new HubConnectionBuilder()
      .withUrl(signalRBaseUrl)
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build();

    this._hubConnection.on('notify', (payload: any) => {
      console.log('SignalR: Received data on dataProxy', payload);
      this.fireDataReceived(payload);
    });

    this._hubConnection
      .onclose(async () => {
        // when disconnected we should display a message and block any user request
        console.log('SignalR Connection lost! Trying to reconnect');
        await this.start();
      });

    return this._hubConnection;
  }

  public async start() {
    try {
      await this._hubConnection.start();
      console.log('SignalR: Connection started!');
    } catch (err) {
      console.log('SignalR: Error while establishing connection :(', err);
      setTimeout(this.start, 5000);
    }
    return this;
  }
}
