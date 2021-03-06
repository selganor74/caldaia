import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule } from '@angular/forms';
import {
  HttpClient,
  HttpClientModule
} from '@angular/common/http';

import { TerminalModule } from 'primeng/terminal';
import { AccordionModule } from 'primeng/primeng';
import { PanelModule } from 'primeng/primeng';
import { ButtonModule } from 'primeng/primeng';
import { RadioButtonModule } from 'primeng/primeng';
import { GrowlModule } from 'primeng/primeng';
import { ToolbarModule } from 'primeng/toolbar';
import { TabViewModule } from 'primeng/tabview';
import { CardModule } from 'primeng/card';
import { DialogModule } from 'primeng/dialog';
import { ChartModule } from 'primeng/chart';
import { CheckboxModule } from 'primeng/checkbox';

import { AppComponent } from './app.component';
import { BackendService } from './backend.service';
import { TerminalComponent } from './terminal/terminal.component';
import { StatsGraphComponent } from './stats-graph/stats-graph.component';
import { TemperatureChartComponent } from './chart/temperature-chart/temperature-chart.component';
import { AccumulatorChartComponent } from './chart/accumulator-chart/accumulator-chart.component';
import { ClientInfoComponent } from './client-info/client-info.component';

@NgModule({
  declarations: [
    AppComponent,
    TerminalComponent,
    StatsGraphComponent,
    TemperatureChartComponent,
    AccumulatorChartComponent,
    ClientInfoComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    FormsModule,
    HttpClientModule,
    AccordionModule,
    PanelModule,
    ButtonModule,
    RadioButtonModule,
    GrowlModule,
    ToolbarModule,
    TabViewModule,
    CardModule,
    DialogModule,
    TerminalModule,
    ChartModule,
    CheckboxModule
  ],
  providers: [HttpClient, BackendService],
  bootstrap: [AppComponent]
})
export class AppModule {

}
