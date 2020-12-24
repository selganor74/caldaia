import {
  Component,
  Input,
  OnInit
} from '@angular/core';

export class TemperatureChartDefinition {
  public header: string;
  public labels: string[] = [];
  public minDataset: number[] = [];
  public maxDataset: number[] = [];
  public avgDataset: number[] = [];
}

class HeaderTranslator {
  TEMPERATURA_ACCUMULO = 'Accumulo';
  TEMPERATURA_CAMINO = 'Camino';
  TEMPERATURA_ACCUMULO_INFERIORE = 'Accumulo Inferiore';
  TEMPERATURA_PANNELLI = 'Pannelli Solari';
}

@Component({
  selector: 'temperature-chart',
  templateUrl: './temperature-chart.component.html',
  styleUrls: ['./temperature-chart.component.css']
})
export class TemperatureChartComponent implements OnInit {

  private translate = new HeaderTranslator();

  @Input()
  public title: string;

  private _chartDefinition: TemperatureChartDefinition;

  @Input('chart-definition')
  public get chartDefinition(): TemperatureChartDefinition { return this._chartDefinition; }
  public set chartDefinition(value: TemperatureChartDefinition ) {
    if (!value) { return; }

    console.log(`TemperatureChartComponent: setting dataFromApi to `, value);
    this._chartDefinition = value;
    this.chartDefinitionToNgPrimeChartData();
  }

  public readonly chartOptions: Chart.ChartOptions = {
    // devicePixelRatio: 4 / 1,
    responsive: true
  };

  public isFullScreen = false;

  // data to be visualized in ngprime chart format.
  public chartData: Chart.ChartData;

  constructor() { }

  ngOnInit() {
  }

  chartDefinitionToNgPrimeChartData() {

    const chartMinDataset: Chart.ChartDataSets = {
      backgroundColor: 'blue',
      borderColor: 'darkblue',
      label: 'T.Min',
      fill: false,
      data: this._chartDefinition.minDataset
    };

    const chartMaxDataset: Chart.ChartDataSets = {
      backgroundColor: 'red',
      borderColor: 'darkred',
      label: 'T.Max',
      fill: false,
      data: this._chartDefinition.maxDataset
    };

    const chartAvgDataset: Chart.ChartDataSets = {
      backgroundColor: 'green',
      borderColor: 'darkgreen',
      label: 'T.Media',
      fill: false,
      lineTension: 0,
      data: this._chartDefinition.avgDataset
    };

    const newChartData: Chart.ChartData = {};

    this.title = this.translate[this._chartDefinition.header] || this._chartDefinition.header;
    newChartData.labels = this._chartDefinition.labels;
    newChartData.datasets = [
    //  chartMinDataset,
    //  chartMaxDataset,
      chartAvgDataset
    ];

    console.log('TemperatureChartComponent: set new chart data', newChartData);
    this.chartData = newChartData;
  }
}
