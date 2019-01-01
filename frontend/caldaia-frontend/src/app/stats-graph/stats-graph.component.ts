import {
  Component,
  OnInit,
  OnDestroy,
//  ChangeDetectorRef
} from '@angular/core';

import { Chart } from 'chart.js';
import { cloneDeep } from 'lodash/fp';

import {
  HttpClient
} from '@angular/common/http';

import { environment } from '../../environments/environment';

import {
  IAccumulatorsTimeSlot,
  IAccumulatorsContent
} from './i-accumulators-time-slot';

import * as moment from 'moment';
import {
  ITemperaturesTimeSlot,
  ITemperaturesContent
} from './i-temperatures-time-slot';

interface IChartData extends Chart.ChartData {
  header: string; // this is not needed by the chart, but by our UI
}

type PossibleDatasets = 'tmax' | 'tmin' | 'tavg';
class TemperatureChartData {
  public chartData: IChartData;
  public availableChartData: { [possibleDataset: string]: Chart.ChartDataSets } = {};

  public setAvailableDataSets(enableDataSets: PossibleDatasets[]) {

    this.chartData.datasets = [];
    this.chartData = cloneDeep(this.chartData);

    for (const ds of enableDataSets) {
      this.chartData.datasets.push(this.availableChartData[ds]);
    }
  }
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
  public allTempChartsData: TemperatureChartData[] = [];
  public commonChartOptions: Chart.ChartOptions = {};

  private _tempDatasetsToShow: PossibleDatasets[] = ['tavg'];

  public get tempDatasetsToShow(): PossibleDatasets[] {
    return this._tempDatasetsToShow;
  }

  public set tempDatasetsToShow(value: PossibleDatasets[]) {
    this._tempDatasetsToShow = value;
    this.setDatasetsToShow();
  }

  constructor(
    private http: HttpClient
    // private changeDetectorRef: ChangeDetectorRef
  ) { }

  ngOnInit() {
    this.apiBaseUrl = environment.apiBaseUrl;

    this.commonChartOptions.devicePixelRatio = 4 / 1;
    this.commonChartOptions.responsive = true;

    this.getDataAndBuildGraphs();

    this.interval = setInterval(() => {
      this.getDataAndBuildGraphs();
    }, 60000);
  }


  private setDatasetsToShow(): any {
    for (const dt of this.allTempChartsData) {
      dt.setAvailableDataSets(this._tempDatasetsToShow);
    }
    const tempArray = this.allTempChartsData;
    this.allTempChartsData = [];

    for (const t of tempArray) {
      this.allTempChartsData.push(t);
    }
    // this.changeDetectorRef.detectChanges();
  }

  private getDataAndBuildGraphs() {
    this.http.get(this.apiBaseUrl + '/statistics/last-24-hours').subscribe((data: string) => {
      const parsedData: IAccumulatorsTimeSlot[] = JSON.parse(data);
      this.buildAllChartsData(parsedData);
    });
    this.http.get(this.apiBaseUrl + '/statistics/last-24-hours-temperatures').subscribe((data: string) => {
      const parsedData: ITemperaturesTimeSlot[] = JSON.parse(data);
      this.buildAllTempChartsData(parsedData);
      this.setDatasetsToShow();
    });
  }

  buildAllTempChartsData(dataFromApi: ITemperaturesTimeSlot[]): any {
    let aContent: ITemperaturesContent;
    let index = dataFromApi.length - 1;

    while (!aContent && index >= 0) {
      aContent = dataFromApi[index].Content;
      index--;
    }

    if (!aContent) { return; }
    this.allTempChartsData = [];
    // foreach property we can build a Graph.
    for (const propertyName in aContent) {
      if (!aContent.hasOwnProperty(propertyName)) { continue; }

      const chartData = this.buildGraphForTemperaturesProperty(dataFromApi, propertyName);
      this.allTempChartsData.push(chartData);
    }
  }

  buildGraphForTemperaturesProperty(dataFromApi: ITemperaturesTimeSlot[], propertyName: string): TemperatureChartData {
    const labels: string[] = [];

    const chartMinDataset: Chart.ChartDataSets = {
      backgroundColor: 'blue',
      borderColor: 'darkblue',
      label: 'T.Min',
      fill: false,
      data: []
    };

    const chartMaxDataset: Chart.ChartDataSets = {
      backgroundColor: 'red',
      borderColor: 'darkred',
      label: 'T.Max',
      fill: false,
      data: []
    };

    const chartAvgDataset: Chart.ChartDataSets = {
      backgroundColor: 'green',
      borderColor: 'darkgreen',
      label: 'T.Media',
      fill: false,
      data: []
    };

    for (const slot of dataFromApi) {
      labels.push(moment(slot.SlotStart).format('HH:mm'));

      const min = slot.Content ? slot.Content[propertyName].Min : undefined;
      const max = slot.Content ? slot.Content[propertyName].Max : undefined;
      const avg = slot.Content ? Math.round(slot.Content[propertyName].Avg * 100) / 100 : undefined;
      (<number[]>chartMinDataset.data).push(min);
      (<number[]>chartMaxDataset.data).push(max);
      (<number[]>chartAvgDataset.data).push(avg);
    }

    const toReturn = new TemperatureChartData();
    toReturn.availableChartData['tmin'] = chartMinDataset;
    toReturn.availableChartData['tmax'] = chartMaxDataset;
    toReturn.availableChartData['tavg'] = chartAvgDataset;

    const chartData: IChartData = {
      header: propertyName,
      labels: labels,
      datasets: []
    };

    toReturn.chartData = chartData;

    return toReturn;
  }

  ngOnDestroy(): void {
    clearInterval(this.interval);
  }

  private buildAllChartsData(dataFromApi: IAccumulatorsTimeSlot[]) {
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

    const chartDataset: Chart.ChartDataSets = {
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
      (<number[]>chartDataset.data).push(toAdd);
      totalTimeOn += toAddNoRounding;
    }

    chartDataset.label = 'Tot: ' + Math.round((totalTimeOn / 1000 / 60) * 10) / 10 + ' min.';

    const chartData: IChartData = {
      header: propertyName,
      labels: labels,
      datasets: [chartDataset]
    };

    return chartData;
  }

}
