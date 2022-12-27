/// <reference lib="webworker" />

import { DataProcessorInData } from "./data-processor-in-data";
import { DataProcessorOutData } from "./data-processor-out-data";

addEventListener('message', ({ data }) => {
  const toReturn : DataProcessorOutData = {};
  const inData: DataProcessorInData = data as DataProcessorInData;
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

