import { Component } from "@angular/core";
import { FormControl, Validators } from "@angular/forms";
import { EmailPasswordAuthProvider } from "../../services/auth/providers/EmailPasswordAuthProvider";
import { MatDialog } from "@angular/material/dialog";
import { InfoPopupDialogComponent } from "../../utility-components/info-popup-dialog/info-popup-dialog.component";
import { MessageDto } from "../../data/Dto/MessageDto";

@Component({
  selector: "reset-password-form",
  templateUrl: "./reset-password-form.component.html",
  styleUrls: ["./reset-password-form.component.css"]
})
export class ResetPasswordFormComponent
{
  private readonly authProvider: EmailPasswordAuthProvider;
  private readonly dialog: MatDialog;

  emailControl = new FormControl("", [Validators.required, Validators.email]);

  constructor(authProvider: EmailPasswordAuthProvider, dialog: MatDialog)
  {
    this.authProvider = authProvider;
    this.dialog = dialog;
  }

  getEmailError(): string | null
  {
    if (this.emailControl.getError("required"))
      return "Email is required";
    if (this.emailControl.getError("email"))
      return "Invalid email";
    return null;
  }
  submit()
  {
    if (!this.emailControl.valid)
      return;
    this.authProvider.requestPasswordReset(<string>this.emailControl.value)
      .subscribe({
        next: this.onResponse.bind(this),
        error: (error) => console.error(error)
    });
  }
  private onResponse(response: MessageDto)
  {
    console.log(response);
    this.dialog.open(InfoPopupDialogComponent, {
      data:
      {
        info: response.message
      }
    });
  }
}
