export interface ISettingsFromArduino {
    timestamp?: Date;
    TEMP_SAMPLING_INTERVAL?: number;
    T_ISTERESI_CALDAIA?: number;
    deltaSolare?: number;
    rotexMaxTempConCamino?: number;
    rotexMinTempConCamino?: number;
    rotexTermoMax?: number;
    rotexTermoMin?: number;
}
