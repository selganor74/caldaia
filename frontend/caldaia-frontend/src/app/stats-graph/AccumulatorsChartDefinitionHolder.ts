import * as moment from 'moment';

import { AccumulatorsChartDefinition } from '../chart/accumulator-chart/accumulator-chart.component';
import { IAccumulatorsTimeSlot } from './i-accumulators-time-slot';

type PossibleTimeUnits = 'ms' | 'sec' | 'min' | 'hr' | 'gg';

export class AccumulatorsChartDefinitionHolder {

    public allCharts = [
        'TEMPO_ACCENSIONE_POMPA_RISCALDAMENTO',
        'TEMPO_ACCENSIONE_POMPA_CAMINO',
        'TEMPO_ACCENSIONE_CALDAIA',
        'TEMPO_TERMOSTATI_AMBIENTE'
    ];
    public chartDefinitions: { [possibleChartId: string]: AccumulatorsChartDefinition } = {};

    private chartTranslation: { [possibleChartId: string]: string } = {
        'TEMPO_ACCENSIONE_POMPA_RISCALDAMENTO': 'Pompa Riscaldamento',
        'TEMPO_ACCENSIONE_POMPA_CAMINO' : 'Pompa Camino',
        'TEMPO_ACCENSIONE_CALDAIA': 'Caldaia Metano',
        'TEMPO_TERMOSTATI_AMBIENTE': 'Stato Termostati'
    };

    constructor(private dataFromApi: IAccumulatorsTimeSlot[]) {
        for (const chartId of this.allCharts) {
            const chart = this.buildGraphForProperty(dataFromApi, chartId);
            this.chartDefinitions[chartId] = chart;
        }
    }

    private buildGraphForProperty(dataFromApi: IAccumulatorsTimeSlot[], propertyName: string): AccumulatorsChartDefinition {
        const labels: string[] = [];
        let totalTimeOnMilliseconds = 0;

        const data: number[] = [];
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
          data.push(toAdd);
          totalTimeOnMilliseconds += toAddNoRounding;
        }
        const tu = this.findSuitableTimeUnit(totalTimeOnMilliseconds);
        const description = 'Tot: ' + Math.round(tu.value * 10) / 10 + ' ' + tu.units + '.';

        const title = this.chartTranslation[propertyName] || propertyName;
        const chartData: AccumulatorsChartDefinition = {
          header: title,
          labels: labels,
          description: description,
          accumulatorValue: data
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
