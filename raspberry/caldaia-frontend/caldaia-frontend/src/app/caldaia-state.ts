export type Measure = {
    name: string;
    uoM: string;
    formattedValue: string;
    digitalValue?: number;
    value: number;
    utcTimeStamp: Date;
}

export type State = { [index: string]: Measure };