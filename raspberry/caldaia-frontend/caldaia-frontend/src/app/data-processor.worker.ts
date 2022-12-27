/// <reference lib="webworker" />

import { Measure } from "./caldaia-state";

export type GraphValues = {
  graphId?: string;
  lastValue?: string;
  labels?: string[];
  values?: number[];
  tOnMilliseconds?: number;
}

export type InData = {
  graphId?: string;
  measures?: Measure[];
  timeSlotSize?: number;
}

addEventListener('message', ({ data }) => {
  const toReturn : GraphValues = {};
  const inData: InData = data as InData;
  if (!inData.measures)
    return;

  const graphValues = inData.measures!;
  
  toReturn.labels = graphValues.map(d => d.utcTimeStamp
    .toLocaleString('it-IT', {
      year: undefined,
      month: undefined,
      day: undefined,
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit'
    }));

  toReturn.lastValue = graphValues[graphValues.length - 1].formattedValue;
  toReturn.values = graphValues.map(d => d.value);
  
  toReturn.tOnMilliseconds = inData.timeSlotSize! * toReturn.values.reduce((sum, val) => sum + val, 0);
  toReturn.graphId = inData.graphId;
  
  postMessage(toReturn);
});

