import { HttpClient } from '@angular/common/http';
import { Component, Input } from '@angular/core';
import { AnalogMeter, Measure } from 'src/app/caldaia-state';
import { v4 as uuid } from 'uuid';

import { ChartItem, Chart, LineController, LineElement, PointElement, LinearScale, Title, CategoryScale } from 'chart.js';

const hourInMilliseconds = 60 * 60 * 1000;
const minuteInMilliseconds = 60 * 1000;

Chart.register(LineController, LineElement, PointElement, LinearScale, Title, CategoryScale);

@Component({
  selector: 'app-temp-chart',
  template: `
<div style=
  "display: inline-block;
    position: relative;
    width: 30vw; 
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
  ctx?: ChartItem;
  chart?: Chart<"line", number[], string>;

  @Input() name!: string;
  @Input('data-endpoint') dataEndpoint!: string;

  lastValue: string = "";

  constructor(
    private httpClient: HttpClient
  ) {
    // generates an unique id for our canvas
    this.id = "A" + uuid() + "A";
    setTimeout(() => this.init(), 10);
    setInterval(() => this.loadData(), 5000);
  }

  loadData(): void {
    this.httpClient.get<AnalogMeter>(this.dataEndpoint)
      .subscribe(result => {
        this.lastValue = result.lastKnownValue.formattedValue;
        const now = Date.now().valueOf();
        const all = result.history.map(d => {
          d.utcTimeStamp = new Date(Date.parse(d.utcTimeStamp.toString()));
          d.value = Number(d.value);
          return d;
        });
        const filtered = all.filter(d => d.utcTimeStamp.valueOf() > (now - 24 * hourInMilliseconds));
        const labels = filtered.map(d => d.utcTimeStamp
          .toLocaleString('it-IT', {
            year: undefined,
            month: undefined,
            day: undefined,
            hour: '2-digit',
            minute: '2-digit',
            second: '2-digit'
          }));

        const fromApi = filtered.map(d => d.value);
        if (!this.chart || !this.chart.data)
          return;

        this.chart.data.labels = labels;
        this.chart.data.datasets[0].data = fromApi;
        this.chart?.update();
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
          label: 'CAMINO_TEMPERATURA',
          data: [],
          pointBorderColor: 'greenyellow',
          pointRadius: 1,
          pointHoverRadius: 3,
          borderWidth: 1,
          showLine: true,
        }]
      },
      options: {
        responsive: true,
        color: "greenyellow",
        datasets: {
          line: {
            fill: { below: "" }
          }
        },
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

  private onDataLoad(data: { utcTimeStamp: Date, value: number }[]) {

  }
}
