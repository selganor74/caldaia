import { TestBed, inject } from '@angular/core/testing';

import { SignalrHandlerService } from './signalr-handler.service';

describe('SignalrHandlerService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [SignalrHandlerService]
    });
  });

  it('should be created', inject([SignalrHandlerService], (service: SignalrHandlerService) => {
    expect(service).toBeTruthy();
  }));
});
