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
  IAccumulatorsContent
} from './i-accumulators-time-slot';

import * as moment from 'moment';
import {
  ITemperaturesTimeSlot,
  ITemperaturesContent} from './i-temperatures-time-slot';

import {
  TemperatureChart
} from './temperature-chart';

import {
  IChartData
} from './i-chart-data';
import { TemperatureChartDefinition } from '../chart/temperature-chart/temperature-chart.component';
import { TemperaturesChartDefinitionHolder } from './TemperaturesChartDefinitionHolder';

export type PossibleDatasets = 'tmax' | 'tmin' | 'tavg';
type PossibleRanges = 'last24hours' | 'lastWeek';
type PossibleTimeUnits = 'ms' | 'sec' | 'min' | 'hr' | 'gg';


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

  public get allTempChartDefinitions(): TemperatureChartDefinition[] {
    const toReturn = [];
    for(let chartId of this.tempChartsData[this._currentRange].allCharts) {
      toReturn.push(this.tempChartsData[this._currentRange].chartDefinitions[chartId])
    }
    return toReturn; 
  }

  private apiBaseUrl: string;
  private interval: any;

  public allAccuChartsData: { [timeRange: string]: IChartData[] } = {};

  public commonChartOptions: Chart.ChartOptions = {};
  public currentFullScreen: TemperatureChart;

  public get showZoom() {
    console.log(`showZoom: ${!!this.currentFullScreen}`);
    return !!this.currentFullScreen;
  }

  private _currentRange: PossibleRanges = 'last24hours';

  public get currentRange(): PossibleRanges {
    return this._currentRange;
  }

  public set currentRange(value: PossibleRanges) {
    this._currentRange = value;
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

  private getDataAndBuildGraphs() {
    this.http.get(this.apiBaseUrl + '/statistics/last-24-hours').subscribe((data: string) => {
      const parsedData: IAccumulatorsTimeSlot[] = JSON.parse(data);

      this.buildAllAccuChartsData(parsedData, 'last24hours');
    });

    this.http.get(this.apiBaseUrl + '/statistics/last-24-hours-temperatures').subscribe((data: string) => {
      const parsedData: ITemperaturesTimeSlot[] = JSON.parse(data);

      this.tempChartsData["last24hours"] = new TemperaturesChartDefinitionHolder(parsedData);
    });

    this.http.get(this.apiBaseUrl + '/statistics/last-week-accumulators').subscribe((data: string) => {
      const parsedData: IAccumulatorsTimeSlot[] = JSON.parse(data);

      this.buildAllAccuChartsData(parsedData, 'lastWeek');
    });
    this.http.get(this.apiBaseUrl + '/statistics/last-week-temperatures').subscribe((data: string) => {
      const parsedData: ITemperaturesTimeSlot[] = JSON.parse(data);

      this.tempChartsData["lastWeek"] = new TemperaturesChartDefinitionHolder(parsedData);
    });
  }

  private onToggleFullScreen(source: TemperatureChart) {
    console.log("Notify Toggle Full Screen called");

    if (this.currentFullScreen === source) {
      this.currentFullScreen = undefined;
      // source.isFullScreen = false;
      return;
    }

    // for(const timeRange in this.allTempChartsData) {
    //   const chartsForTimeRange = this.allTempChartsData[timeRange];
    //   for(const chart of chartsForTimeRange)
    //     chart.isFullScreen = false;
    // }
    // source.isFullScreen = true;

    this.currentFullScreen = source;
  }

  buildGraphForTemperaturesProperty(dataFromApi: ITemperaturesTimeSlot[], propertyName: string): TemperatureChart {
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
      labels.push(moment(slot.SlotStart).format('MM/DD HH:mm'));

      const min = slot.Content ? slot.Content[propertyName].Min : undefined;
      const max = slot.Content ? slot.Content[propertyName].Max : undefined;
      const avg = slot.Content ? Math.round(slot.Content[propertyName].Avg * 100) / 100 : undefined;
      (<number[]>chartMinDataset.data).push(min);
      (<number[]>chartMaxDataset.data).push(max);
      (<number[]>chartAvgDataset.data).push(avg);
    }

    const toReturn = new TemperatureChart(this.onToggleFullScreen);

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

  private buildAllAccuChartsData(dataFromApi: IAccumulatorsTimeSlot[], timeRange: PossibleRanges) {
    let aContent: IAccumulatorsContent;
    let index = dataFromApi.length - 1;

    while (!aContent && index >= 0) {
      aContent = dataFromApi[index].Content;
      index--;
    }

    if (!aContent) { return; }
    this.allAccuChartsData[timeRange] = [];
    // foreach property we can build a Graph.
    for (const propertyName in aContent) {
      if (!aContent.hasOwnProperty(propertyName)) { continue; }

      const chartData = this.buildGraphForProperty(dataFromApi, propertyName);
      this.allAccuChartsData[timeRange].push(chartData);
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

    let totalTimeOnMilliseconds = 0;

    for (const slot of dataFromApi) {
      labels.push(moment(slot.SlotEnd).format('MM/DD HH:mm'));
      const slotSizeInMilliseconds = this.calculateMillisecondsFromHHmmss(slot.SlotSize);
      const slotSizeInMinutes = slotSizeInMilliseconds / 1000 / 60;
      // const slotSizeInHours = slotSizeInMinutes / 60;
      const toAddNoRounding = slot.Content ? slot.Content[propertyName] : 0;
      const toAdd = slot.Content
        ? Math.round(
          Math.min(
            (slot.Content[propertyName] / slotSizeInMilliseconds),
            1
          ) * slotSizeInMinutes * 100
        ) / 100
        : 0;
      (<number[]>chartDataset.data).push(toAdd);
      totalTimeOnMilliseconds += toAddNoRounding;
    }
    const tu = this.findSuitableTimeUnit(totalTimeOnMilliseconds);
    chartDataset.label = 'Tot: ' + Math.round(tu.value * 10) / 10 + ' ' + tu.units + '.';

    const chartData: IChartData = {
      header: propertyName,
      labels: labels,
      datasets: [chartDataset]
    };

    return chartData;
  }

  calculateMillisecondsFromHHmmss(slotSizeHHmmss: string): number {
    const hms = slotSizeHHmmss.split(':');
    const hours = Number.parseInt(hms[0]);
    const minutes = Number.parseInt(hms[1]);
    const seconds = Number.parseInt(hms[2]);
    return (hours * (60 * 60) + minutes * 60 + seconds) * 1000;
  }

  findSuitableTimeUnit(timeMilliseconds: number): { units: PossibleTimeUnits; value: number } {
    const toReturn = { units: <PossibleTimeUnits>'ms', value: timeMilliseconds };
    while (true) {
      switch (toReturn.units) {
        case 'ms': {
          if (toReturn.value >= 0 && toReturn.value < 1000) { return toReturn; }
          toReturn.units = 'sec';
          toReturn.value /= 1000;
          break;
        }

        case 'sec': {
          if (toReturn.value >= 1 && toReturn.value < 60) { return toReturn; }
          toReturn.units = 'min';
          toReturn.value /= 60;
          break;
        }

        case 'min': {
          if (toReturn.value >= 1 && toReturn.value < 60) { return toReturn; }
          toReturn.units = 'hr';
          toReturn.value /= 60;
          break;
        }

        case 'hr': {
          if (toReturn.value >= 1 && toReturn.value < 24) { return toReturn; }
          toReturn.units = 'gg';
          toReturn.value /= 24;
          break;
        }

        default: {
          return toReturn;
        }
      }
    }
  }

}
