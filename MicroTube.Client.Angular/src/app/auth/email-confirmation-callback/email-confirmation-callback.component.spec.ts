import { ComponentFixture, TestBed } from "@angular/core/testing";

import { EmailConfirmationCallbackComponent } from "./email-confirmation-callback.component";

describe("EmailConfirmationCallbackComponent", () => {
  let component: EmailConfirmationCallbackComponent;
  let fixture: ComponentFixture<EmailConfirmationCallbackComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ EmailConfirmationCallbackComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EmailConfirmationCallbackComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
