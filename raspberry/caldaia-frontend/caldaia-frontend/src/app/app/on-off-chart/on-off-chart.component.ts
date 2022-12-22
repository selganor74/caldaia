import { Component } from '@angular/core';

import { Chart } from 'chart.js'
import { BaseChartComponent, templateBuilder } from '../base-chart';

const template = templateBuilder(`
  <span class="outline-text" style="font-size: 9vh;">{{lastValue}} </span>
  <span class="outline-text" style="font-size: 5vh;">tₒₙ: </span>
  <span class="outline-text" style="font-size: 7vh;">{{this.tOn}}</span>
`);

@Component({
  selector: 'app-on-off-chart',
  template: template,
  styles: [
  ]
})
export class OnOffChartComponent extends BaseChartComponent {

  protected override chartBuilder() : Chart<"line", number[], string>{
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
}
