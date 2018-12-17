import { IDataFromArduino } from './idata-from-arduino';
import { ISettingsFromArduino } from './isettings-from-arduino';

import { environment } from '../environments/environment';

export class SignalRIntegration {


    private dataProxy: any; // SignalR.Hub.Proxy;
    private settingsProxy: any; // SignalR.Hub.Proxy;
    private _stateChangedHandlers: ((state: number, stateDescription: string) => void)[] = [];
    private _dataReceivedHandlers: ((data: IDataFromArduino) => void)[] = [];
    private _settingsReceivedHandlers: ((settings: ISettingsFromArduino) => void)[] = [];


    constructor(
        private _hubConnection: any // SignalR.Hub.Connection;
    ) {
        this.configSignalR();
    }

    public onSettingsReceived(handler: (settings: ISettingsFromArduino) => void) {
        if (handler) {
            this._settingsReceivedHandlers.push(handler);
        }
        return this;
    }

    private fireSettingsReceived(settings: ISettingsFromArduino) {
        for (const handler of this._settingsReceivedHandlers) {
            try {
                handler(settings);
            } catch (err) {
                console.error('fireSettingsReceived: Errors while executing handler', handler.toString());
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
                handler(data);
            } catch (err) {
                console.error('fireDataReceived: Errors while executing handler', handler.toString());
            }
        }
    }

    public onStateChanged(handler: (state: number, stateDescription: string) => void) {
        if (handler) {
            this._stateChangedHandlers.push(handler);
        }
        return this;
    }

    private fireStateChanged(state: number, stateDescription: string) {
        for (const handler of this._stateChangedHandlers) {
            try {
                handler(state, stateDescription);
            } catch (err) {
                console.error('fireStateChanged: Errors while executing handler', handler.toString());
            }
        }
    }

    private configSignalR() {
        this._hubConnection.reconnectDelay = 2000;
        this._hubConnection.stateChanged((state) => {

            const stateConversion = { 0: 'connecting', 1: 'connected', 2: 'reconnecting', 4: 'disconnected' };
            const stateDescription = stateConversion[state.newState];

            console.log('SignalR: Connection status changed to ' + stateDescription);
            this.fireStateChanged(state.newState, stateDescription);
        });

        this.dataProxy = this._hubConnection.createHubProxy('data');
        this.settingsProxy = this._hubConnection.createHubProxy('settings');

        this.dataProxy.on('notify', (payload: any) => {
            console.log('SignalR: Received data on dataProxy', payload);
            this.fireDataReceived(payload);
        });

        this.settingsProxy.on('notify', (payload: any) => {
            console.log('SignalR: Received data on settingsProxy', payload);
            this.fireSettingsReceived(payload);
        });

        this._hubConnection
            .disconnected(() => {
                // when disconnected we should display a message and block any user request
                console.log('SignalR Connection lost! Trying to reconnect');
                this.start();
            });
    }

    public start() {
        this._hubConnection
            .start()
            .done(() => {
                console.log('SignalR: Connection started!');
            })
            .catch(err => {
                console.log('SignalR: Error while establishing connection :(', err);
            });
        return this;
    }
}
