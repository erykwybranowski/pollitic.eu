import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SupportGraphComponentComponent } from './support-graph.component';

describe('SupportGraphComponentComponent', () => {
  let component: SupportGraphComponentComponent;
  let fixture: ComponentFixture<SupportGraphComponentComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SupportGraphComponentComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SupportGraphComponentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
