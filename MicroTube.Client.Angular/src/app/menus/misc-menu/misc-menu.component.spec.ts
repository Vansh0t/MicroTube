import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MiscMenuComponent } from './misc-menu.component';

describe('MiscMenuComponent', () => {
  let component: MiscMenuComponent;
  let fixture: ComponentFixture<MiscMenuComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MiscMenuComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(MiscMenuComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
