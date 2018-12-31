import { ITimeSlot } from './i-time-slot';

export interface IAccumulatorsContent {
    TEMPO_ACCENSIONE_POMPA_RISCALDAMENTO: number;
    TEMPO_ACCENSIONE_POMPA_CAMINO: number;
    TEMPO_ACCENSIONE_CALDAIA: number;
    TEMPO_TERMOSTATI_AMBIENTE: number;
    TEMPO_TERMOSTATO_ACCUMULATORE: number;
    TEMPO_ACCENSIONE_POMPA_SOLARE: number;
}

export interface IAccumulatorsTimeSlot extends ITimeSlot < IAccumulatorsContent > {
}
