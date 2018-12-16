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

  decrementTIsteresiCaldaia(): any {
    return this.http.post(this.apiBaseUrl + '/settings/t-isteresi-caldaia/decrement', {});
  }

  incrementTIsteresiCaldaia(): any {
    return this.http.post(this.apiBaseUrl + '/settings/t-isteresi-caldaia/increment', {});
  }

  decrementTempSamplingInterval(): any {
    return this.http.post(this.apiBaseUrl + '/settings/temp-sampling-interval/decrement', {});
  }

  incrementTempSamplingInterval(): any {
    return this.http.post(this.apiBaseUrl + '/settings/temp-sampling-interval/increment', {});
  }

  decrementMinTempConCamino(): any {
    return this.http.post(this.apiBaseUrl + '/settings/min-temp-con-camino/decrement', {});
  }

  incrementMinTempConCamino(): any {
    return this.http.post(this.apiBaseUrl + '/settings/min-temp-con-camino/increment', {});
  }

  decrementMaxTempConCamino(): any {
    return this.http.post(this.apiBaseUrl + '/settings/max-temp-con-camino/decrement', {});
  }

  incrementMaxTempConCamino(): any {
    return this.http.post(this.apiBaseUrl + '/settings/max-temp-con-camino/increment', {});
  }

  public getLatestData(): Observable<IDataFromArduino> {
    return <Observable<IDataFromArduino>>this.http.get(this.apiBaseUrl + '/queries/latestdata');
  }

  public updateLatestData() {
    return this.http.post(this.apiBaseUrl + '/commands/get', {});
  }

  public updateLatestSettings() {
    return this.http.post(this.apiBaseUrl + '/commands/reloadsettings', {});
  }
}
