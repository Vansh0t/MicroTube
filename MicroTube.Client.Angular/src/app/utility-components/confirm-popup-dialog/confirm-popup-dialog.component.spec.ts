import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InfoPopupDialogComponent } from './confirm-popup-dialog.component';

describe('ConfirmPopupDialogComponent', () => {
  let component: ConfirmPopupDialogComponent;
  let fixture: ComponentFixture<InfoPopupDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ConfirmPopupDialogComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ConfirmPopupDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
