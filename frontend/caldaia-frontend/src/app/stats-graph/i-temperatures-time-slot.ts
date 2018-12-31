import { ITimeSlot } from './i-time-slot';

export interface IAggregatedValues {
    Max: number;
    Min: number;
    Avg: number;
}

export interface ITemperaturesContent {
    TEMPERATURA_ACCUMULO: IAggregatedValues;
    TEMPERATURA_PANNELLI: IAggregatedValues;
    TEMPERATURA_CAMINO: IAggregatedValues;
    TEMPERATURA_ACCUMULO_INFERIORE: IAggregatedValues;
}

export interface ITemperaturesTimeSlot extends ITimeSlot<ITemperaturesContent> {

}
