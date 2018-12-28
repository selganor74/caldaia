import { IDataFromArduino } from './idata-from-arduino';
import { ISettingsFromArduino } from './isettings-from-arduino';

export class SignalRIntegration {

    private dataProxy: any; // SignalR.Hub.Proxy;
    private settingsProxy: any; // SignalR.Hub.Proxy;
    private logsProxy: any; // SignalR.Hub.Proxy;
    private rawProxy: any; // SignalR.Hub.Proxy

    private _stateChangedHandlers: ((state: number, stateDescription: string) => void)[] = [];
    private _dataReceivedHandlers: ((data: IDataFromArduino) => void)[] = [];
    private _settingsReceivedHandlers: ((settings: ISettingsFromArduino) => void)[] = [];
    private _rawReceivedHandlers: ((data: string) => void)[] = [];


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

    public unregisterHandler(handler: Function) {
        const handlersArray: Array<Array<Function>> = [
            this._settingsReceivedHandlers,
            this._stateChangedHandlers,
            this._dataReceivedHandlers
        ];

        for (const ha of handlersArray) {
            const toRemove = ha.indexOf(handler);
            if ( toRemove !== -1 ) {
                console.log('unregistering handler', handler);
                ha.splice(toRemove, 1);
            }
        }
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

    public onRawReceived(handler: (raw: string) => void) {
        if (handler) {
            this._rawReceivedHandlers.push(handler);
        }
        return this;
    }

    private fireRawReceived(raw: string) {
        for (const handler of this._rawReceivedHandlers) {
            try {
                handler(raw);
            } catch (err) {
                console.error('fireRawReceived: Errors while executing handler', handler.toString());
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
        this.logsProxy = this._hubConnection.createHubProxy('logs');
        this.rawProxy = this._hubConnection.createHubProxy('raw');

        this.rawProxy.on('notify', (raw: string) => {
            // console.log('SignalR: Received raw string on rawProxy', raw );
            this.fireRawReceived(raw);
        });

        this.dataProxy.on('notify', (payload: any) => {
            console.log('SignalR: Received data on dataProxy', payload);
            this.fireDataReceived(payload);
        });

        this.settingsProxy.on('notify', (payload: any) => {
            console.log('SignalR: Received data on settingsProxy', payload);
            this.fireSettingsReceived(payload);
        });

        this.logsProxy.on('trace', (payload: any) => {
            // tslint:disable-next-line:no-console
            console.log('SignalR-Logs::Trace', payload);
        });

        this.logsProxy.on('info', (payload: any) => {
            console.log('SignalR-Logs', payload);
        });

        this.logsProxy.on('warning', (payload: any) => {
            console.warn('SignalR-Logs', payload);
        });

        this.logsProxy.on('error', (payload: any) => {
            console.error('SignalR-Logs', payload);
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
