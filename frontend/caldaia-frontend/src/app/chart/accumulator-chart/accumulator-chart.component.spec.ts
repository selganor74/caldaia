import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AccumulatorChartComponent } from './accumulator-chart.component';

describe('AccumulatorChartComponent', () => {
  let component: AccumulatorChartComponent;
  let fixture: ComponentFixture<AccumulatorChartComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AccumulatorChartComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AccumulatorChartComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
