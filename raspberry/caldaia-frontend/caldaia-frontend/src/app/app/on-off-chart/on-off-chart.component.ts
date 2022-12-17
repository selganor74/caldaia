import { Attribute, Component } from '@angular/core';
import { Measure } from 'src/app/caldaia-state';
import { v4 as uuid } from 'uuid';

import { ChartItem, Chart } from 'chart.js'
import { Subscription } from 'rxjs';
import { DeviceDataLoaderService, DeviceDataLoaderServiceFactory, hourInMilliseconds, minuteInMilliseconds, secondInMilliseconds } from 'src/app/device-data-loader.service';

@Component({
  selector: 'app-on-off-chart',
  template: `
<div style="
    display: inline-block;
    position: relative;
    width: 100%;
    ">
  <div style="
    display: block;
    align: center;
    text-align: center;
    font-familiy: monospace;
    width: 100%; 
    height: 2em;
    ">
    <span>{{name}}: </span><span style="font-size: 2em">{{lastValue}} </span><span> tₒₙ: </span><span style="font-size: 2em">{{this.tOn}}</span>  
  </div>
  <canvas id="{{this.id}}" ></canvas>
</div>
  `,
  styles: [
  ]
})
export class OnOffChartComponent {
  public id: string;
  ctx!: ChartItem;
  chart!: Chart<"line", number[], string>;
  currentData: Measure[] = [];
  dataLoader: DeviceDataLoaderService;

  lastValue: string = "";

  tOnMilliseconds: number = 0;
  private readonly timeSlotSize = 5 * minuteInMilliseconds;

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

    @Attribute('data-endpoint')
    public dataEndpoint: string,

    dataLoaderFactory: DeviceDataLoaderServiceFactory
  ) {
    // generates an unique id for our canvas
    this.id = "gid-" + uuid();

    this.dataLoader = dataLoaderFactory.createLoader(this.dataEndpoint, "digital", this.timeSlotSize, 24 * hourInMilliseconds);

    setTimeout(() => this.init(), 10);
    setInterval(() => this.loadData(), 5000);
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

    this.chart = new Chart(this.ctx, {
      type: 'line',
      data: {
        datasets: [{
          label: "",
          data: [],
          fill: {
            target: 'origin',
            below: 'rgba(0,40,0,100)',
            above: 'rgba(0,40,0,100)'
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
}
