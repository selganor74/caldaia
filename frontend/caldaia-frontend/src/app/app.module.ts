import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpClientModule } from '@angular/common/http';

import { AccordionModule } from 'primeng/primeng';
import { PanelModule } from 'primeng/primeng';
import { ButtonModule } from 'primeng/primeng';
import { RadioButtonModule } from 'primeng/primeng';
import { GrowlModule } from 'primeng/primeng';
import {ToolbarModule} from 'primeng/toolbar';

import { AppComponent } from './app.component';
import { BackendService } from './backend.service';

@NgModule({
  declarations: [
    AppComponent
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
    ToolbarModule
  ],
  providers: [HttpClient, BackendService],
  bootstrap: [AppComponent]
})
export class AppModule {

}
