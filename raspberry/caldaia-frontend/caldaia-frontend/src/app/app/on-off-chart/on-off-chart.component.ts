import { HttpClient } from '@angular/common/http';
import { Attribute, Component, Input } from '@angular/core';
import { DigitalMeter, Measure } from 'src/app/caldaia-state';
import { v4 as uuid } from 'uuid';

import { slottifyMeasures } from 'src/app/measure-helpers';
import { ChartItem, Chart } from 'chart.js'
import { Subscription } from 'rxjs';
import { DeviceDataLoaderService, DeviceDataLoaderServiceFactory } from 'src/app/device-data-loader.service';


const hourInMilliseconds = 60 * 60 * 1000;
const minuteInMilliseconds = 60 * 1000;


@Component({
  selector: 'app-on-off-chart',
  template: `
<div style=
  "display: inline-block;
    position: relative;
    width: 31.5vw; 
    height: 30vh;">
  <div style=
  "display: block;
    align: center;
    text-align: center;
    width: 100%; 
    height: 2em;">
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

    this.dataLoader = dataLoaderFactory.createLoader(this.dataEndpoint, "analog", 5 * minuteInMilliseconds, 0.5*hourInMilliseconds);

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

        const fromApi = graphValues.map(d => d.value);
        if (!this.chart || !this.chart.data)
          return;

        this.lastValue = graphValues[fromApi.length -1].formattedValue;
        
        this.chart.data.labels = labels;
        this.chart.data.datasets[0].data = fromApi;

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
            display: false
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
