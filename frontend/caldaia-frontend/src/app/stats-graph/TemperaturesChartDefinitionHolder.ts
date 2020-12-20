import * as moment from 'moment';
import {
  ITemperaturesTimeSlot,

  IAggregatedValues
} from './i-temperatures-time-slot';
import { TemperatureChartDefinition } from '../chart/temperature-chart/temperature-chart.component';

export class TemperaturesChartDefinitionHolder {

  public allCharts = ['TEMPERATURA_ACCUMULO', 'TEMPERATURA_PANNELLI', 'TEMPERATURA_CAMINO', 'TEMPERATURA_ACCUMULO_INFERIORE'];
  public chartDefinitions: { [possibleChartId: string]: TemperatureChartDefinition; } = {};

  constructor(private dataFromApi: ITemperaturesTimeSlot[]) {
    for (const ts of dataFromApi) {
      const label = moment(ts.SlotStart).format("MM/DD HH:mm");
      for (let chartId of this.allCharts) {

        this.chartDefinitions[chartId] = this.chartDefinitions[chartId] || new TemperatureChartDefinition();
        let chartDef = this.chartDefinitions[chartId];

        chartDef.header = chartId;

        chartDef.labels.push(label);
        chartDef.avgDataset.push((<IAggregatedValues>(ts.Content[chartId])).Avg);
        chartDef.minDataset.push((<IAggregatedValues>(ts.Content[chartId])).Min);
        chartDef.maxDataset.push((<IAggregatedValues>(ts.Content[chartId])).Max);
      }
    }
  }
}
