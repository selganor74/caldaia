import { Measure } from "./caldaia-state";

export function slottifyMeasures(timeSlotSize: number, currentData: Measure[], fillToInMilliseconds: number = 0 ): Measure[] {
    let slotEnd: number = 0;
    let slotStart: number = 0;
    let idx = 0;
    const toReturn: Measure[] = [];

    const alignToTimeSlot = (dateTimeAsNumber: number, timeSlotSize: number) => {
        //
        // |----- timeSlotSize -----|----- timeSlotSize -----|
        //                return -> |-- % timeSlotSize --| <- dateTimeAsNumber
        return dateTimeAsNumber - (dateTimeAsNumber % timeSlotSize);
    };

    while (idx < currentData.length) {
        if (!currentData[idx]) {
            idx++;
            continue;
        }

        slotStart = slotEnd || alignToTimeSlot(currentData[idx].utcTimeStamp.valueOf(), timeSlotSize);
        slotEnd = slotStart + timeSlotSize;
        const slotEndDate = new Date(slotEnd);
        const newSlot = [];
        // console.log(`slotStart:${slotStart}, slotEnd:${slotEnd}`);
        while (idx < currentData.length) {
            const actual = currentData[idx];
            if (actual) {
                if (actual.utcTimeStamp.valueOf() >= slotEnd)
                    break;
                newSlot.push(currentData[idx]);
            }
            idx++;
        }

        if (newSlot.length == 0) {
            const lastSlotValue = toReturn[toReturn.length - 1];
            if (lastSlotValue) {
                const repeated = Object.assign({}, lastSlotValue);
                repeated.utcTimeStamp = new Date(slotEnd);
                toReturn.push(repeated);
            }
            continue;
        }
        console.log(`slot elements: ${newSlot.length}`);
        console.log(newSlot);

        let graphValue = Object.assign({}, newSlot[0]);
        graphValue.value = 0;
        newSlot.forEach(e => graphValue.value += e.value);
        graphValue.value /= newSlot.length;
        graphValue.utcTimeStamp = slotEndDate;

        toReturn.push(graphValue);
    }

    // fills the remaining slots with the last measure
    while (fillToInMilliseconds > slotEnd) {
        slotStart = slotEnd || alignToTimeSlot(currentData[idx].utcTimeStamp.valueOf(), timeSlotSize);
        slotEnd = slotStart + timeSlotSize;
        const lastSlotValue = toReturn[toReturn.length - 1] || 
        <Measure>{
            formattedValue: "", 
            name: "", 
            uoM: "", 
            utcTimeStamp: new Date(slotEnd), 
            value: 0 
        };

        if (lastSlotValue) {
            const repeated = Object.assign({}, lastSlotValue);
            repeated.utcTimeStamp = new Date(slotEnd);
            toReturn.push(repeated);
        }
    }

    return toReturn;
}
