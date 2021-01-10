import {
  Component,
  OnInit
} from '@angular/core';

import {
  HttpClient
} from '@angular/common/http';

import { environment } from '../../environments/environment';

interface IClientInfo {
  clientId: string;
  clientIP: string;
  clientHostName: string;
}

@Component({
  selector: 'app-client-info',
  templateUrl: './client-info.component.html',
  styleUrls: ['./client-info.component.css']
})
export class ClientInfoComponent implements OnInit {

  clientIp: string;
  clientHostName: string;

  constructor(
    private http: HttpClient
  ) { }

  ngOnInit() {
    this.getClientInfo();

    setInterval(
      this.getClientInfo.bind(this),
      10000
    );
  }

  getClientInfo() {
    this.http.get(environment.apiBaseUrl + '/client-info').subscribe((data: IClientInfo) => {

      this.clientIp = data.clientIP;
      this.clientHostName = data.clientHostName;
    });
  }
}
