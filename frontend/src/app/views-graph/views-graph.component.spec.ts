import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ViewsGraphComponent } from './views-graph.component';

describe('PartyGraphComponent', () => {
  let component: ViewsGraphComponent;
  let fixture: ComponentFixture<ViewsGraphComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ViewsGraphComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ViewsGraphComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
