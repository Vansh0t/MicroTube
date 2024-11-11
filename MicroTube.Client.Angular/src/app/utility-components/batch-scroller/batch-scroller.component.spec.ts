import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BatchScrollerComponent } from './batch-scroller.component';

describe('BatchScrollerComponent', () => {
  let component: BatchScrollerComponent;
  let fixture: ComponentFixture<BatchScrollerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [BatchScrollerComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(BatchScrollerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
