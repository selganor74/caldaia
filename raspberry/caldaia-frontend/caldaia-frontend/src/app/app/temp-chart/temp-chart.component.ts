import { Component } from '@angular/core';
import { Chart } from 'chart.js';
import { BaseChartComponent, templateBuilder } from '../base-chart';

const template = templateBuilder(`
<span style="text-shadow: -1px -1px 0 greenyellow, 1px -1px 0 greenyellow, -1px 1px 0 greenyellow, 1px 1px 0 greenyellow; 
             font-size: 11vh; 
             color: rgba(0,40,0,0.99)"
>{{lastValue}}</span>  
`);

@Component({
  selector: 'app-temp-chart',
  template: template,
  styles: [
  ]
})
export class TempChartComponent extends BaseChartComponent {
  protected override chartBuilder(): Chart<"line", number[], string> {
    return new Chart(this.ctx, {
      type: 'line',
      data: {
        datasets: [{
          label: this.name,
          data: [],
          fill: {
            target: 'origin',
            below: 'rgba(40,0,0,0.75)',
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

/*
@Component({
  selector: 'app-temp-chart',
  template: `
  <div style="
    display: inline-block;
    position: relative;
    margin: 0;
    padding: 0;
    width: 100%;
    ">
    <canvas style="box-sizing: border-box; margin:0; padding:0;" id="{{this.id}}" ></canvas>
    <div style="
      font-size: 16vh; 
      display: inline-block; 
      position: absolute; 
      left:0; top:0;
      align: center; 
      margin-top: 4vh; 
      padding: 0; 
      vertical-align: center; 
      text-align: center; 
      font-familiy: monospace; 
      width: 100%; 
      height: 100%;
      ">
  <span style="
    text-shadow: -1px -1px 0 greenyellow, 1px -1px 0 greenyellow, -1px 1px 0 greenyellow, 1px 1px 0 greenyellow; 
    font-size: 11vh; 
    color: rgba(0,40,0,0.99)">{{lastValue}}</span>  
  </div>
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
          fill: {
            target: 'origin',
            below: 'rgba(40,0,0,0.75)',
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

 */