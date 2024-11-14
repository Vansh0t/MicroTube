import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CommentsAreaComponent } from './comments-area.component';

describe('CommentsAreaComponent', () => {
  let component: CommentsAreaComponent;
  let fixture: ComponentFixture<CommentsAreaComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [CommentsAreaComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(CommentsAreaComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
