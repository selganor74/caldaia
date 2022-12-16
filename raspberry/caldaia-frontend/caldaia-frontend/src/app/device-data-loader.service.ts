import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { from, map, Observable, Observer } from 'rxjs';
import { AnalogMeter, DigitalMeter, Measure } from 'src/app/caldaia-state';
import { slottifyMeasures } from './measure-helpers';

export type DeviceType = "digital" | "analog";

export const minuteInMilliseconds = 60 * 1000;
export const hourInMilliseconds = 60 * minuteInMilliseconds;

@Injectable({
  providedIn: 'root',
})
export class DeviceDataLoaderServiceFactory {
  constructor(
    private httpClient: HttpClient
  ) {
  }

  public createLoader(
    deviceName: string,
    deviceType: DeviceType,
    timeSlotSizeInMilliseconds: number,
    timeWindowSizeInMilliseconds: number): DeviceDataLoaderService {
    const toReturn = new DeviceDataLoaderService(this.httpClient);

    toReturn.setDeviceName(deviceName, deviceType);
    toReturn.setTimeSlot(timeSlotSizeInMilliseconds);
    toReturn.setTimeWindow(timeWindowSizeInMilliseconds);

    return toReturn;
  }
}


export class DeviceDataLoaderService {
  private baseUrl: string = "../api/meters";
  private deviceName!: string;
  private deviceType!: DeviceType;
  lastValue!: string;
  lastTimestamp: Date = new Date(Date.now());
  private isFirstLoad: boolean = true;
  private allData: Measure[] = [];
  private timeSlotSizeInMilliseconds: number = 10 * minuteInMilliseconds / 60;
  private timeWindowSizeInMilliseconds: number = 24 * hourInMilliseconds;

  private get meterUrl(): string { return `${this.baseUrl}/${this.deviceType}/${this.deviceName}`; };
  private get historyUrl(): string { return `${this.baseUrl}/${this.deviceType}/${this.deviceName}/history/${this.lastTimestamp.toISOString()}`; };

  constructor(
    private httpClient: HttpClient
  ) { }

  setDeviceName(deviceName: string, deviceType: DeviceType) {
    this.deviceName = deviceName;
    this.deviceType = deviceType;
  }

  setTimeSlot(timeSlotSizeInMilliseconds: number) {
    this.timeSlotSizeInMilliseconds = timeSlotSizeInMilliseconds;
  }

  setTimeWindow(timeWindowInMilliseconds: number) {
    this.timeWindowSizeInMilliseconds = timeWindowInMilliseconds;
  }

  public loadData(): Observable<Measure[]> {
    if (!this.deviceName)
      return from([]);

    let o: Observable<Measure[]>;

    if (this.isFirstLoad) {
      o = this.firstDataLoad();
      this.isFirstLoad = false;
    } else {
      o = this.deltaDataLoad();
    }

    return o.pipe(map(() => this.allData));
  }

  private deltaDataLoad(): Observable<Measure[]> {
    const o = this.httpClient.get<Measure[]>(this.historyUrl);

    return o.pipe(map(result => {
      return this.processData(result);
    }));
  }

  private firstDataLoad(): Observable<Measure[]> {
    const o = this.httpClient.get<AnalogMeter | DigitalMeter>(this.meterUrl);

    return o.pipe(map(result => {
      const history = result?.history || [];
      return this.processData(history);
    }));
  }

  private processData(fromApi: Measure[]): Measure[] {
    let lastValue = fromApi[fromApi.length - 1];
    this.lastValue = lastValue?.formattedValue || "";
    this.lastTimestamp = lastValue?.utcTimeStamp || new Date(Date.now());

    const managed = fromApi.map(d => {
      d.utcTimeStamp = new Date(Date.parse(d.utcTimeStamp.toString()));
      d.value = Number(d.value);
      return d;
    });

    this.allData.push(...managed);
    const now = Date.now();
    const timeWindowStart = now - this.timeWindowSizeInMilliseconds;
    this.allData = this.allData.filter(d => d.utcTimeStamp.valueOf() > timeWindowStart);
    
    const preWindowElements = this.allData
      .filter(d => d.utcTimeStamp.valueOf() <= timeWindowStart)
    const firstElement = preWindowElements[preWindowElements.length - 1];
    if (firstElement) {
      firstElement.utcTimeStamp = new Date(timeWindowStart);
      this.allData.unshift(firstElement);
    }

    const processed = slottifyMeasures(this.timeSlotSizeInMilliseconds, this.allData, now);

    return processed;
  }
}
