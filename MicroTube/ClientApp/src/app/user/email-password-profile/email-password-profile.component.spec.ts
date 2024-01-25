import { ComponentFixture, TestBed } from "@angular/core/testing";

import { EmailPasswordProfileComponent } from "./email-password-profile.component";

describe("EmailPasswordProfileComponent", () => {
  let component: EmailPasswordProfileComponent;
  let fixture: ComponentFixture<EmailPasswordProfileComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [EmailPasswordProfileComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EmailPasswordProfileComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
