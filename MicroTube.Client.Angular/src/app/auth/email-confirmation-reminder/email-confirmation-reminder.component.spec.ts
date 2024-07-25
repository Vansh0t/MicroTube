import { ComponentFixture, TestBed } from "@angular/core/testing";

import { EmailConfirmationReminderComponent } from "./email-confirmation-reminder.component";

describe("EmailConfirmationReminderComponent", () => {
  let component: EmailConfirmationReminderComponent;
  let fixture: ComponentFixture<EmailConfirmationReminderComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ EmailConfirmationReminderComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EmailConfirmationReminderComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
