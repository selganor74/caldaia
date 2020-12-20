import { Chart } from 'chart.js';


export interface IChartData extends Chart.ChartData {
  header: string; // this is not needed by the chart, but by our UI
}
