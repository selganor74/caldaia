import { cloneDeep } from 'lodash/fp';
import { PossibleDatasets } from './stats-graph.component';
import { IChartData } from "./i-chart-data";

export class TemperatureChart {
  public chartData: IChartData;
  public availableChartData: { [possibleDataset: string]: Chart.ChartDataSets; } = {};

  public isFullScreen = false;

  private _notifyToggleFullScreen: (source: TemperatureChart) => void;

  constructor(notifyFullScreen: (source: TemperatureChart) => void) {
    this._notifyToggleFullScreen = notifyFullScreen || ((src) => { });
  }

  public setAvailableDataSets(enableDataSets: PossibleDatasets[]) {

    this.chartData.datasets = [];
    this.chartData = cloneDeep(this.chartData);

    for (const ds of enableDataSets) {
      this.chartData.datasets.push(this.availableChartData[ds]);
    }
  }

  public toggleFullScreen() {
    this._notifyToggleFullScreen(this);
  }
}
