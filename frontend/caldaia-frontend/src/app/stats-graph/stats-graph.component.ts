import {
  Component,
  OnInit,
  OnDestroy
} from '@angular/core';

import {
  HttpClient,
  HttpHeaders
} from '@angular/common/http';

import { environment } from '../../environments/environment';

import { IAccumulatorsTimeSlot, IAccumulatorsContent } from './i-accumulators-time-slot';

import * as moment from 'moment';

interface IDataset {
  label: string;
  backgroundColor: string;
  borderColor: string;
  data: number[];
}

interface IChartData {
  header: string; // this is not needed by the chart, but by our UI
  labels: string[];
  datasets: IDataset[];
}

@Component({
  selector: 'app-stats-graph',
  templateUrl: './stats-graph.component.html',
  styleUrls: ['./stats-graph.component.css']
})
export class StatsGraphComponent implements OnInit, OnDestroy {
  private apiBaseUrl: string;
  private interval: any;

  public allChartsData: IChartData[] = [];

  constructor(
    private http: HttpClient
  ) { }

  ngOnInit() {
    this.apiBaseUrl = environment.apiBaseUrl;

    this.getDataAndBuildGraphs();

    this.interval = setInterval(() => {
      this.getDataAndBuildGraphs();
    }, 60000 );
  }

  private getDataAndBuildGraphs() {
    this.http.get(this.apiBaseUrl + '/statistics/last-24-hours').subscribe((data: string) => {
      const parsedData: IAccumulatorsTimeSlot[] = JSON.parse(data);
      this.buildAllChartsData(parsedData);
    });
  }

  ngOnDestroy(): void {
    clearInterval(this.interval);
  }

  private buildAllChartsData(dataFromApi: IAccumulatorsTimeSlot[]) {
    const available_data: string[] = [];
    let aContent: IAccumulatorsContent;
    let index = dataFromApi.length - 1;

    while (!aContent && index >= 0) {
      aContent = dataFromApi[index].Content;
      index--;
    }

    if (!aContent) { return; }
    this.allChartsData = [];
    // foreach property we can build a Graph.
    for (const propertyName in aContent) {
      if (!aContent.hasOwnProperty(propertyName)) { continue; }

      const chartData = this.buildGraphForProperty(dataFromApi, propertyName);
      this.allChartsData.push(chartData);
    }
  }

  private buildGraphForProperty(dataFromApi: IAccumulatorsTimeSlot[], propertyName: string): IChartData {
    const labels: string[] = [];

    const chartDataset: IDataset = {
      backgroundColor: 'darkgrey',
      borderColor: 'black',
      label: '',
      data: []
    };

    let totalTimeOn = 0;

    for (const slot of dataFromApi) {
      labels.push(moment(slot.SlotStart).format('HH:mm'));
      const slotSize = 15 * 60 * 1000;
      const toAddNoRounding = slot.Content ? slot.Content[propertyName] : 0;
      const toAdd = slot.Content ? Math.round(Math.min((slot.Content[propertyName] / slotSize) * 15, 15) * 100) / 100 : 0;
      chartDataset.data.push(toAdd);
      totalTimeOn += toAddNoRounding;
    }

    chartDataset.label =  'Tot: ' + Math.round( ( totalTimeOn / 1000 / 60 ) * 10 ) / 10 + ' min.';

    const chartData: IChartData = {
      header: propertyName,
      labels: labels,
      datasets: [chartDataset]
    };

    return chartData;
  }

}
