import { Measure } from "./caldaia-state";


export type DataProcessorInData = {
  graphId?: string;
  measures?: Measure[];
  timeSlotSize?: number;
};
