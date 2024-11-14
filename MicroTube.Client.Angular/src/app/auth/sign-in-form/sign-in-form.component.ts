import { Component } from "@angular/core";
import { FormControl, FormGroup, Validators } from "@angular/forms";
import { EmailPasswordAuthProvider } from "../../services/auth/providers/EmailPasswordAuthProvider";
import { AuthManager } from "../../services/auth/AuthManager";
import { SignInCredentialPasswordDto } from "../../data/Dto/SignInCredentialPasswordDto";
import { HttpErrorResponse } from "@angular/common/http";

@Component({
  selector: "sign-in-form",
  templateUrl: "./sign-in-form.component.html",
  styleUrls: ["./sign-in-form.component.css"]
})
export class SignInFormComponent
{
  readonly MIN_LENGTH = 2;
  readonly credentialControl = new FormControl("", [Validators.required, Validators.minLength(this.MIN_LENGTH)]);
  readonly passwordControl = new FormControl("", [Validators.required]);
  readonly rememberMeControl = new FormControl(true);
  readonly formGroup = new FormGroup([this.credentialControl, this.passwordControl, this.rememberMeControl]);

  private readonly authProvider: EmailPasswordAuthProvider;
  private readonly authManager: AuthManager;
  constructor(authManager: AuthManager, authProvider: EmailPasswordAuthProvider)
  {
    this.authManager = authManager;
    this.authProvider = authProvider;
  }

  getCredentialError(): string | null
  {
    let error = "";
    if (this.credentialControl.hasError("required"))
      error += "Email or Username is required. ";
    if (this.credentialControl.hasError("minLength"))
      error += `Email or Username require at least ${this.MIN_LENGTH} characters. `;
    return error != "" ? error : null;
  }
  getPasswordError(): string | null
  {
    if (this.passwordControl.hasError("required"))
      return "Password is required";
    return null;
  }
  getServerError(): string | null
  {
    if (this.formGroup.hasError("serverError"))
      return this.formGroup.getError("serverError");
    return null;
  }
  submit(): void
  {
    if (!this.formGroup.valid)
      return;
    const signInData = new SignInCredentialPasswordDto(<string>this.credentialControl.value, <string>this.passwordControl.value);
    this.authProvider.signInData = signInData;
    this.authManager.signIn(this.authProvider, <boolean>this.rememberMeControl.value, this.onServerError.bind(this));
  }
  onServerError(error: HttpErrorResponse)
  {
    console.log(error.error);
    this.formGroup.setErrors({ serverError: error.error });
  }
}
