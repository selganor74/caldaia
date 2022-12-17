import { Attribute, Component, Input } from '@angular/core';
import { Measure } from 'src/app/caldaia-state';
import { v4 as uuid } from 'uuid';

import { ChartItem, Chart } from 'chart.js';
import { DeviceDataLoaderService, DeviceDataLoaderServiceFactory, hourInMilliseconds, minuteInMilliseconds } from 'src/app/device-data-loader.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-temp-chart',
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
    <span>{{name}} : </span><span style="font-size: 2em">{{lastValue}}</span>  
  </div>
  <canvas id="{{this.id}}" ></canvas>
</div>
  `,
  styles: [
  ]
})
export class TempChartComponent {
  public id: string;
  ctx!: ChartItem;
  chart!: Chart<"line", number[], string>;
  currentData: Measure[] = [];
  dataLoader: DeviceDataLoaderService;

  lastValue: string = "";

  constructor(
    @Attribute('name')
    public name: string,

    @Attribute('data-endpoint')
    public dataEndpoint: string,

    dataLoaderFactory: DeviceDataLoaderServiceFactory
  ) {
    // generates an unique id for our canvas
    this.id = "gid-" + uuid();

    this.dataLoader = dataLoaderFactory.createLoader(this.dataEndpoint, "analog", 5 * minuteInMilliseconds, 24 * hourInMilliseconds);

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

        this.lastValue = graphValues[fromApi.length - 1].formattedValue;

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
          label: this.name,
          data: [],
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
            beginAtZero: false,
            grid: {
              color: 'darkgreen',
            }
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
