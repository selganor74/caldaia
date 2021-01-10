import {
  Component,
  OnInit,
  OnDestroy,
  //  ChangeDetectorRef
} from '@angular/core';

import {
  HttpClient
} from '@angular/common/http';

import { environment } from '../../environments/environment';

import {
  IAccumulatorsTimeSlot,
} from './i-accumulators-time-slot';

import {
  ITemperaturesTimeSlot,
} from './i-temperatures-time-slot';


import { TemperatureChartDefinition } from '../chart/temperature-chart/temperature-chart.component';
import { TemperaturesChartDefinitionHolder } from './TemperaturesChartDefinitionHolder';
import { AccumulatorsChartDefinition } from '../chart/accumulator-chart/accumulator-chart.component';
import { AccumulatorsChartDefinitionHolder } from './AccumulatorsChartDefinitionHolder';

export type PossibleDatasets = 'tmax' | 'tmin' | 'tavg';
type PossibleRanges = 'last24hours' | 'lastWeek';


@Component({
  selector: 'app-stats-graph',
  templateUrl: './stats-graph.component.html',
  styleUrls: ['./stats-graph.component.css']
})
export class StatsGraphComponent implements OnInit, OnDestroy {

  public tempChartsData: {[timeRange: string]: TemperaturesChartDefinitionHolder} = {
    last24hours: new TemperaturesChartDefinitionHolder([]),
    lastWeek: new TemperaturesChartDefinitionHolder([])
  };

  public accuChartsData: {[timeRange: string]: AccumulatorsChartDefinitionHolder} = {
    last24hours: new AccumulatorsChartDefinitionHolder([]),
    lastWeek: new AccumulatorsChartDefinitionHolder([])
  };

  public get allTempChartDefinitions(): TemperatureChartDefinition[] {
    const toReturn = [];
    for (const chartId of this.tempChartsData[this.currentRange].allCharts) {
      toReturn.push(this.tempChartsData[this.currentRange].chartDefinitions[chartId]);
    }
    return toReturn;
  }

  public get allAccuChartDefinitions(): AccumulatorsChartDefinition[] {
    const toReturn = [];
    for (const chartId of this.accuChartsData[this.currentRange].allCharts) {
      toReturn.push(this.accuChartsData[this.currentRange].chartDefinitions[chartId]);
    }
    return toReturn;
  }

  private apiBaseUrl: string;
  private interval: any;

  public currentRange: PossibleRanges = 'last24hours';

  constructor(
    private http: HttpClient
    // private changeDetectorRef: ChangeDetectorRef
  ) { }

  ngOnInit() {
    this.apiBaseUrl = environment.apiBaseUrl;

    this.getDataAndBuildGraphs();

    this.interval = setInterval(() => {
      this.getDataAndBuildGraphs();
    }, 60000);
  }

  private getDataAndBuildGraphs() {
    this.http.get(this.apiBaseUrl + '/statistics/last-24-hours').subscribe((data: string) => {
      const parsedData: IAccumulatorsTimeSlot[] = JSON.parse(data);

      this.accuChartsData['last24hours'] = new AccumulatorsChartDefinitionHolder(parsedData);
    });

    this.http.get(this.apiBaseUrl + '/statistics/last-24-hours-temperatures').subscribe((data: string) => {
      const parsedData: ITemperaturesTimeSlot[] = JSON.parse(data);

      this.tempChartsData['last24hours'] = new TemperaturesChartDefinitionHolder(parsedData);
    });

    this.http.get(this.apiBaseUrl + '/statistics/last-week-accumulators').subscribe((data: string) => {
      const parsedData: IAccumulatorsTimeSlot[] = JSON.parse(data);

      this.accuChartsData['lastWeek'] = new AccumulatorsChartDefinitionHolder(parsedData);
    });
    
    this.http.get(this.apiBaseUrl + '/statistics/last-week-temperatures').subscribe((data: string) => {
      const parsedData: ITemperaturesTimeSlot[] = JSON.parse(data);

      this.tempChartsData['lastWeek'] = new TemperaturesChartDefinitionHolder(parsedData);
    });
  }

  ngOnDestroy(): void {
    clearInterval(this.interval);
  }
}
