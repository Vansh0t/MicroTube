import { ComponentFixture, TestBed } from "@angular/core/testing";

import { PasswordChangeFormComponent } from "./password-change-form.component";

describe("PasswordChangeFormComponent", () => {
  let component: PasswordChangeFormComponent;
  let fixture: ComponentFixture<PasswordChangeFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ PasswordChangeFormComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PasswordChangeFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
