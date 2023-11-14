import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InfoPopupDialogComponent } from './info-popup-dialog.component';

describe('InfoPopupDialogComponent', () => {
  let component: InfoPopupDialogComponent;
  let fixture: ComponentFixture<InfoPopupDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ InfoPopupDialogComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(InfoPopupDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
