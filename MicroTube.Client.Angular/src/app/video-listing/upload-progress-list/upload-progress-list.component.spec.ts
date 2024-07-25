import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UploadProgressListComponent } from './upload-progress-list.component';

describe('UploadProgressListComponent', () => {
  let component: UploadProgressListComponent;
  let fixture: ComponentFixture<UploadProgressListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ UploadProgressListComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(UploadProgressListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
