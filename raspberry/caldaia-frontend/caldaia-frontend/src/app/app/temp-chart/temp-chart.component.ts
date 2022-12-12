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
  lastTimestamp!: Date;
  currentData: Measure[] = [];

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
    if (!this.lastTimestamp) {
      this.httpClient.get<AnalogMeter>(this.dataEndpoint)
        .subscribe(result => {

          this.dataEndpoint += "/history/{fromDate}";

          this.lastValue = result.lastKnownValue.formattedValue;
          const history = result.history;
          this.updateChart(history);
        });
    } else {
      this.httpClient.get<Measure[]>(this.dataEndpoint.replace("{fromDate}", this.lastTimestamp.toISOString()))
        .subscribe(result => {
          if (!result)
            return;

          if (result.length == 0)
            return;

          this.updateChart(result);
        });
    }
  }

  private updateChart(history: Measure[]) {
    const now = Date.now().valueOf();
    const all = history.map(d => {
      d.utcTimeStamp = new Date(Date.parse(d.utcTimeStamp.toString()));
      d.value = Number(d.value);
      return d;
    });
    const lastValue = all[all.length - 1]

    this.lastValue = lastValue.formattedValue;
    this.lastTimestamp = lastValue.utcTimeStamp;

    //const filtered = all.filter(d => d.utcTimeStamp.valueOf() > (now - 24 * hourInMilliseconds));
    this.currentData.push(...all);
    this.currentData = this.currentData.filter(d => d.utcTimeStamp.valueOf() > (now - 24 * hourInMilliseconds));
    const labels = this.currentData.map(d => d.utcTimeStamp
      .toLocaleString('it-IT', {
        year: undefined,
        month: undefined,
        day: undefined,
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit'
      }));

    const fromApi = this.currentData.map(d => d.value);
    if (!this.chart || !this.chart.data)
      return;

    this.chart.data.labels = labels;
    this.chart.data.datasets[0].data = fromApi;

    this.chart.update();
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
