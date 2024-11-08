import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PollingGraphComponent } from './polling-graph.component';

describe('PollingGraphComponent', () => {
  let component: PollingGraphComponent;
  let fixture: ComponentFixture<PollingGraphComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PollingGraphComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PollingGraphComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
