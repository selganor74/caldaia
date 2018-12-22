import {Injectable} from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';

import {IDataFromArduino} from './idata-from-arduino';
import { Observable } from 'rxjs';

import { environment } from '../environments/environment';

const httpOptions = {
    headers: new HttpHeaders({ 'Content-Type': 'application/json' })
};
@Injectable()
export class BackendService {

  apiBaseUrl: string = environment.apiBaseUrl;

  constructor(private http: HttpClient) { }

  public sendString(command: string) {
    const cmd = { ToSend: command };
    return this.http.post(this.apiBaseUrl + '/commands/send-string', cmd).subscribe();
  }

  public getLatestData(): Observable<IDataFromArduino> {
    return <Observable<IDataFromArduino>>this.http.get(this.apiBaseUrl + '/queries/latestdata');
  }

  public updateLatestData() {
    return this.http.post(this.apiBaseUrl + '/commands/get', {}).subscribe();
  }

  public updateLatestSettings() {
    return this.http.post(this.apiBaseUrl + '/commands/reloadsettings', {}).subscribe();
  }

  saveSettings(): any {
    return this.http.post(this.apiBaseUrl + '/settings/save', {}).subscribe();
  }

  public decrementRotexTermoMin(): any {
    return this.http.post(this.apiBaseUrl + '/settings/rotex-termo-min/decrement', {}).subscribe();
  }
  public incrementRotexTermoMin(): any {
    return this.http.post(this.apiBaseUrl + '/settings/rotex-termo-min/increment', {}).subscribe();
  }
  public decrementRotexTermoMax(): any {
    return this.http.post(this.apiBaseUrl + '/settings/rotex-termo-max/decrement', {}).subscribe();
  }
  public incrementRotexTermoMax(): any {
    return this.http.post(this.apiBaseUrl + '/settings/rotex-termo-max/increment', {}).subscribe();
  }

  decrementTIsteresiCaldaia(): any {
    return this.http.post(this.apiBaseUrl + '/settings/t-isteresi-caldaia/decrement', {}).subscribe();
  }

  incrementTIsteresiCaldaia(): any {
    return this.http.post(this.apiBaseUrl + '/settings/t-isteresi-caldaia/increment', {}).subscribe();
  }

  decrementTempSamplingInterval(): any {
    return this.http.post(this.apiBaseUrl + '/settings/temp-sampling-interval/decrement', {}).subscribe();
  }

  incrementTempSamplingInterval(): any {
    return this.http.post(this.apiBaseUrl + '/settings/temp-sampling-interval/increment', {}).subscribe();
  }

  decrementMinTempConCamino(): any {
    return this.http.post(this.apiBaseUrl + '/settings/min-temp-con-camino/decrement', {}).subscribe();
  }

  incrementMinTempConCamino(): any {
    return this.http.post(this.apiBaseUrl + '/settings/min-temp-con-camino/increment', {}).subscribe();
  }

  decrementMaxTempConCamino(): any {
    return this.http.post(this.apiBaseUrl + '/settings/max-temp-con-camino/decrement', {}).subscribe();
  }

  incrementMaxTempConCamino(): any {
    return this.http.post(this.apiBaseUrl + '/settings/max-temp-con-camino/increment', {}).subscribe();
  }

  /** pauses the poller for "seconds" seconds */
  pausePoller(seconds: number): any {
    return this.http.post(this.apiBaseUrl + '/commands/pause-poller', {PauseForSeconds: seconds}).subscribe();
  }
}
