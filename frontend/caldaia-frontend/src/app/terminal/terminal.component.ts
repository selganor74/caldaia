import {
  Component,
  OnInit,
  OnDestroy
} from '@angular/core';

import { TerminalService } from 'primeng/components/terminal/terminalservice';
import { BackendService } from '../backend.service';
import { SignalrHandlerService } from '../signalr-handler.service';

@Component({
  selector: 'app-terminal',
  templateUrl: './terminal.component.html',
  styleUrls: ['./terminal.component.css'],
  providers: [TerminalService, BackendService, SignalrHandlerService]
})
export class TerminalComponent implements OnInit, OnDestroy {

  private _timer: any;
  private _dataHandler: (raw: string) => void;
  private _response = '';
  private _timeout: any;

  constructor(
    private _terminalService: TerminalService,
    private _backendService: BackendService,
    private _signalrHandlerService: SignalrHandlerService
  ) { }

  ngOnInit() {
    this.keepPollerPaused();

    this._terminalService.commandHandler.subscribe(command => {
      console.log('Sending command');
      return this._backendService.sendString(command);
    });

    this._dataHandler = this.showData.bind(this);
    this._signalrHandlerService.registerHandlerForOnRaw(this._dataHandler);
  }

  ngOnDestroy(): void {
    this._signalrHandlerService.unregisterHandler(this._dataHandler);
    console.log('restoring poller.');
    clearInterval(this._timer);
  }

  private showData(data: string) {
    this._response += data;
    if (!this._timeout) {
      this._timeout = setTimeout(() => {
        this._terminalService.sendResponse(this._response);
        this._response = '';
        this._timeout = undefined;
      },
        2500
      );
    }
  }

  private keepPollerPaused() {
    this.pausePoller();
    // pausing poller
    this._timer = setInterval(() => {
      this.pausePoller();
    }, 10000);
  }

  private pausePoller() {
    console.log('pausing poller');
    this._backendService.pausePoller(12);
  }
}
