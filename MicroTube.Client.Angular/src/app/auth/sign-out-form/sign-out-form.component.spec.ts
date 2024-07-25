import { ComponentFixture, TestBed } from "@angular/core/testing";

import { SignOutFormComponent } from "./sign-out-form.component";

describe("SignOutFormComponent", () => {
  let component: SignOutFormComponent;
  let fixture: ComponentFixture<SignOutFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SignOutFormComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SignOutFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
