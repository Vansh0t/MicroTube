import { Component, OnDestroy } from "@angular/core";
import { EmailPasswordAuthProvider } from "../../services/auth/providers/EmailPasswordAuthProvider";
import { Subscription } from "rxjs";
import { MatSnackBar } from "@angular/material/snack-bar";
import { HttpErrorResponse } from "@angular/common/http";

@Component({
  selector: "email-confirmation-reminder",
  templateUrl: "./email-confirmation-reminder.component.html",
  styleUrls: ["./email-confirmation-reminder.component.css"]
})
export class EmailConfirmationReminderComponent implements OnDestroy
{
  private readonly authProvider: EmailPasswordAuthProvider;
  private readonly snackbar: MatSnackBar;
  private requestSubscription: Subscription | null = null;
  constructor(authProvider: EmailPasswordAuthProvider, snackbar: MatSnackBar)
  {
    this.snackbar = snackbar;
    this.authProvider = authProvider;
  }
  resendConfirmationEmail()
  {
    this.unsubscribeRequest();
    const request = this.authProvider.resendEmailConfirmation();
    this.requestSubscription = request.subscribe({
      next: () => { this.snackbar.open("Email sent!", "Ok", { duration: 4000 }); },
      error: (error: HttpErrorResponse) => { this.snackbar.open("Failed to send an email! " + error.error, "Ok", { duration: 4000 }); console.error(error); }
    });
  }
  ngOnDestroy()
  {
    this.unsubscribeRequest();
  }
  private unsubscribeRequest()
  {
    if (this.requestSubscription != null)
    {
      this.requestSubscription.unsubscribe();
    }
  }
}
