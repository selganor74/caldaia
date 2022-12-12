export type Measure = {
    name: string;
    uoM: string;
    formattedValue: string;
    digitalValue?: number;
    value: number;
    utcTimeStamp: Date;
}

export type State = { [index: string]: Measure };

export type AnalogMeter = {
    name: string,
    lastKnownValue: Measure,
    average: Measure,
    history: Measure[]
}