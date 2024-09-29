import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PartyGraphComponent } from './party-graph.component';

describe('PartyGraphComponent', () => {
  let component: PartyGraphComponent;
  let fixture: ComponentFixture<PartyGraphComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PartyGraphComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PartyGraphComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
