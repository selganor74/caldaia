import { Attribute, ChangeDetectorRef, Component } from "@angular/core";
import { Subscription } from "rxjs";
import { Chart, ChartItem } from "chart.js";
import { v4 as uuid } from 'uuid';

import { Measure } from "../caldaia-state";
import {
    DeviceDataLoaderService,
    DeviceDataLoaderServiceFactory,
    hourInMilliseconds,
    minuteInMilliseconds
} from "../device-data-loader.service";

export function templateBuilder(textContent: string) {
    return `
<style>  
.widget-mode {
    display: inline-block;
    position: relative;
    margin: 0;
    padding: 0;
    width: 100%;
}

.full-screen-mode {
    display: block;
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    margin: 0;
    padding: 0;
    width: 100vw;
    height: 100vh;
    z-index: 1;
}

.full-screen-toggle-button {
    position: absolute; 
    right: 0; 
    top: 0;
    float: right; 
    width: 20px; 
    height: 20px;
    cursor: pointer;
    margin: 0;
    font-size: 1em;
    padding: 3px;
    background-color: rgba(0,0,0,.25)
    color: greenyellow;
}


.full-screen-toggle-button-fullscreen {
    z-index: 2;
    position: absolute;
    right: 80px; 
    top: 0;
    float: right; 
    width: 80px; 
    height: 80px;
    cursor: pointer;
    margin: 0;
    font-size: 2em;
    padding: 12px;
    background-color: rgba(0,0,0,.25)
    color: greenyellow;
}


.text-container {
    font-size: 16vh; 
    display: inline-block; 
    position: absolute; 
    left:0; top:0;
    align: center; 
    margin-top: 3vh; 
    padding: 0; 
    vertical-align: center; 
    text-align: center; 
    font-familiy: monospace; 
    width: 100%; 
    height: 100%;
}

.outline-text {
    text-shadow: -1px -1px 0 greenyellow, 1px -1px 0 greenyellow, -1px 1px 0 greenyellow, 1px 1px 0 greenyellow; 
    color: rgba(0,40,0,0.99)
}

.backdrop {
    z-index: -1;
    position: absolute;
    left: 0;
    right: 0;
    top: 0;
    bottom: 0;
    background-color: rgba(0,0,0,0.75);
}

.backdrop-fullscreen {
    z-index: -1;
    position: fixed;
}
</style>
<div class="{{class}}">
    <canvas style="box-sizing: border-box; margin:0; padding:0;" id="{{this.id}}" ></canvas>
    <div class="{{fullScreen ? 'full-screen-toggle-button-fullscreen' : 'full-screen-toggle-button'}}" (click)="toggleFullScreen()">[{{fullScreen ? '-' : '+' }}]</div>
    <div class="text-container">
    ${textContent}
    </div>
    <div class="backdrop {{fullScreen ? 'backdrop-fullscreen' : ''}}">&nbsp;</div>
`;
}



@Component({
    selector: 'app-replace-with-your-own',
    template: templateBuilder(``),
    styles: []
})
export class BaseChartComponent {
    public id: string;
    ctx!: ChartItem;
    chart!: Chart<"line", number[], string>;
    currentData: Measure[] = [];
    dataLoader: DeviceDataLoaderService;
    lastValue: string = "";

    tOnMilliseconds: number = 0;
    private readonly timeSlotSize = 5 * minuteInMilliseconds;
    fullScreen: boolean = false;
    // dataProcessor: Worker;
    get class() {
        return this.fullScreen ? "full-screen-mode" : "widget-mode"
    };

    get tOn() {
        const totalSeconds = this.tOnMilliseconds / 1000;
        let toReturn = Math.round(10 * totalSeconds) / 10;
        let um = "s";

        if (totalSeconds > 60) {
            toReturn = Math.round(10 * totalSeconds / 60) / 10;
            um = "m";
        }

        if (totalSeconds > 3600) {
            toReturn = Math.round(10 * totalSeconds / 3600) / 10;
            um = "h";
        }

        return `${toReturn} ${um}`;
    }

    constructor(
        @Attribute('name')
        public name: string,

        @Attribute('measure-name')
        public measureName: string,

        @Attribute('measure-kind')
        public measureKind: "analog" | "digital",

        private cdr: ChangeDetectorRef,
        dataLoaderFactory: DeviceDataLoaderServiceFactory
    ) {
        // generates an unique id for our canvas
        this.id = "gid-" + uuid();

        this.dataLoader = dataLoaderFactory.createLoader(this.measureName, this.measureKind, this.timeSlotSize, 24 * hourInMilliseconds);
        // this.dataProcessor = new Worker(new URL('./app/data-processor.worker.ts', import.meta.url));
        // // Create a new
        // this.dataProcessor.onmessage = ({ data }) => {
        //     console.log(`page got message: ${data}`);
        // };
        // this.dataProcessor.postMessage('hello');

        setTimeout(() => this.init(), 10);
        setInterval(() => this.loadData(), 5000);
    }

    public toggleFullScreen() {
        this.fullScreen = !this.fullScreen;
        this.cdr.markForCheck();
    }

    loadData(): Subscription {
        return this.dataLoader.loadData()
            .subscribe((graphValues) => {
                const labels = graphValues.map(d => d.utcTimeStamp
                    .toLocaleString('it-IT', {
                        year: undefined,
                        month: undefined,
                        day: undefined,
                        hour: '2-digit',
                        minute: '2-digit',
                        second: '2-digit'
                    }));

                this.lastValue = graphValues[graphValues.length - 1].formattedValue;
                const values = graphValues.map(d => d.value);
                if (!this.chart || !this.chart.data)
                    return;

                this.tOnMilliseconds = this.timeSlotSize * values.reduce((sum, val) => sum + val, 0);

                this.chart.data.labels = labels;
                this.chart.data.datasets[0].data = values;

                this.chart.update();
            });
    }

    private init() {
        this.ctx = <HTMLCanvasElement>document.getElementById(this.id);

        if (!this.ctx)
            return;

        this.chart = this.chartBuilder();
    }

    protected chartBuilder(): Chart<"line", number[], string> {
        return new Chart(this.ctx, {
            type: 'line',
            data: {
                datasets: [{
                    label: this.name,
                    data: [],
                    fill: {
                        target: 'origin',
                        below: 'rgba(0,40,0,0.75)',
                        above: 'rgba(0,40,0,0.75)'
                    },
                    pointBorderColor: 'greenyellow',
                    borderColor: 'green',
                    pointRadius: 1,
                    pointHoverRadius: 3,
                    borderWidth: 1,
                    showLine: true,
                }]
            },
            options: {
                responsive: true,
                color: "greenyellow",
                scales: {
                    y: {
                        beginAtZero: true,
                        grid: {
                            color: 'darkgreen',
                        },
                        display: false,
                        min: 0,
                        max: 1
                    },
                    x: {
                        grid: {
                            color: 'darkgreen',
                        }
                    }
                }
            }
        });
    }
}; 
