import { Measure } from "./caldaia-state";

export function slottifyMeasures(
                    timeSlotSize: number, 
                    currentData: Measure[], 
                    fillToInMilliseconds: number = 0
                ): Measure[] {
    let slotEnd: number = 0;
    let slotStart: number = 0;
    const toReturn: Measure[] = [];

    fillToInMilliseconds = fillToInMilliseconds || Date.now();

    const alignToTimeSlot = (dateTimeAsNumber: number) => {
        //
        // |----- timeSlotSize -----|----- timeSlotSize -----|
        //                return -> |-- % timeSlotSize --| <- dateTimeAsNumber
        return dateTimeAsNumber - (dateTimeAsNumber % timeSlotSize);
    };

    if (currentData.length == 0)
        return [];

    slotStart = alignToTimeSlot(currentData[0].utcTimeStamp.valueOf());
    let lastOneOrDefault: Measure | null = null;

    while (slotEnd < fillToInMilliseconds) {
        slotEnd = slotStart + timeSlotSize;

        const slotEndDate = new Date(slotEnd);
        const newSlot: Measure[] = [];
        const fromData = currentData.filter(d => d.utcTimeStamp.valueOf() < slotEnd && d.utcTimeStamp.valueOf() >= slotStart)

        newSlot.push(...fromData);

        lastOneOrDefault = Object.assign( {}, fromData[fromData.length - 1] || lastOneOrDefault || {
            formattedValue: "",
            name: "",
            uoM: "",
            utcTimeStamp: slotEndDate,
            value: 0
        });
        lastOneOrDefault.utcTimeStamp = slotEndDate;

        // no elements in the slot, so we take the last one or a default
        if (newSlot.length == 0)
            toReturn.push(lastOneOrDefault);

        // if there are elements in the slot we compute the average
        if (newSlot.length > 0) {
            const averageValue = Object.assign({}, newSlot[0]);
            const avg = newSlot.map(d => d.value).reduce((p, v) => { return p += v / newSlot.length }, 0);
            averageValue.value = Math.round( avg * 100 ) / 100;
            averageValue.utcTimeStamp = slotEndDate;

            toReturn.push(averageValue);
        }

        slotStart = slotEnd;
    }

    return toReturn;
}
