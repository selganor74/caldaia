import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { TempChartComponent } from './app/temp-chart/temp-chart.component';
import { OnOffChartComponent } from './app/on-off-chart/on-off-chart.component';

@NgModule({
  declarations: [
    AppComponent,
    TempChartComponent,
    OnOffChartComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    CommonModule,
    HttpClientModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
