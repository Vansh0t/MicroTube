import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LikeComponentComponent } from './like-component.component';

describe('LikeComponentComponent', () => {
  let component: LikeComponentComponent;
  let fixture: ComponentFixture<LikeComponentComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LikeComponentComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(LikeComponentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
