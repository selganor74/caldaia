import { Component, Input, OnInit } from '@angular/core';

export class AccumulatorsChartDefinition {
  public header: string;
  public description: string;
  public labels: string[] = [];
  public accumulatorValue: number[] = [];
}

@Component({
  selector: 'accumulator-chart',
  templateUrl: './accumulator-chart.component.html',
  styleUrls: ['./accumulator-chart.component.css']
})
export class AccumulatorChartComponent implements OnInit {

  public readonly chartOptions: Chart.ChartOptions = {
    // devicePixelRatio: 4 / 1,
    responsive: true
  };

  @Input()
  public title: string;

  private _chartDefinition: AccumulatorsChartDefinition;

  @Input('chart-definition')
  public get chartDefinition(): AccumulatorsChartDefinition {
    return this._chartDefinition;
  }
  public set chartDefinition(value: AccumulatorsChartDefinition) {
    if (!value) { return; }
    this.chartData = this.buildNgPrimeChart(value);
    this._chartDefinition = value;
  }

  public chartData: Chart.ChartData;

  constructor() { }

  ngOnInit() {
  }

  private buildNgPrimeChart(chartDefinition: AccumulatorsChartDefinition): Chart.ChartData {

    this.title = chartDefinition.header;

    const chartDataset: Chart.ChartDataSets = {
      backgroundColor: 'darkgrey',
      borderColor: 'black',
      label: chartDefinition.description,
      data: chartDefinition.accumulatorValue
    };

    const chartData: Chart.ChartData = {
      labels: chartDefinition.labels,
      datasets: [chartDataset]
    };

    return chartData;
  }
}
