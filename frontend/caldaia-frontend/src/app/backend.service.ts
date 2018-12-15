import {Injectable} from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';

import {IDataFromArduino} from './idata-from-arduino';
import { Observable } from 'rxjs';

const httpOptions = {
    headers: new HttpHeaders({ 'Content-Type': 'application/json' })
};
@Injectable()
export class BackendService {

  constructor(private http: HttpClient) { }

  public getLatestData(): Observable<IDataFromArduino> {
    return <Observable<IDataFromArduino>>this.http.get('http://localhost:32767/api/queries/latestdata');
  }

  public updateLatestData() {
    return this.http.get('http://localhost:32767/api/commands/get');
  }

  public updateLatestSettings() {
    return this.http.get('http://localhost:32767/api/commands/reloadsettings');
  }
}
