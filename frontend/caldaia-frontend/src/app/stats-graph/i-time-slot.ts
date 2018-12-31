export interface ITimeSlot<T> {
    SlotEnd: Date;
    SlotSize: string;
    SlotStart: Date;
    Content: T;
}
