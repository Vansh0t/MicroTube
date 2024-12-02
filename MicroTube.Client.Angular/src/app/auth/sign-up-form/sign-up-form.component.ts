import { Component, Input } from "@angular/core";
import { FormControl, FormGroup, Validators } from "@angular/forms";
import { DefaultAuthValidators } from "../../services/validation/DefaultAuthValidators";
import { AuthManager } from "../../services/auth/AuthManager";
import { EmailPasswordAuthProvider } from "../../services/auth/providers/EmailPasswordAuthProvider";
import { SignUpEmailPasswordDto } from "../../data/Dto/SignUpEmailPasswordDto";
import { Subscription } from "rxjs";
import { Router } from "@angular/router";
import { HttpErrorResponse } from "@angular/common/http";

@Component({
  selector: "sign-up-form",
  templateUrl: "./sign-up-form.component.html",
  styleUrls: ["./sign-up-form.component.css"]
})
export class SignUpFormComponent {

  @Input() redirectOnSignedIn: boolean = true;
  readonly formGroup: FormGroup;
  readonly usernameControl: FormControl;
  readonly emailControl: FormControl;
  readonly passwordControl: FormControl;
  readonly passwordConfirmationControl: FormControl;
  readonly rememberMeControl: FormControl;
  signUpSubscription: Subscription | null = null;

  private readonly authManager: AuthManager;
  private readonly authProvider: EmailPasswordAuthProvider;
  private readonly MIN_USERNAME_LENGTH: number;
  private readonly MAX_USERNAME_LENGTH: number;
  private readonly MIN_PASSWORD_LENGTH: number;
  private readonly MAX_PASSWORD_LENGTH: number;

  private authStateSubscription: Subscription | null = null;
  private readonly router: Router;
  constructor(authValidators: DefaultAuthValidators, authManager: AuthManager, authProvider: EmailPasswordAuthProvider, router: Router)
  {
    this.authManager = authManager;
    this.authProvider = authProvider;
    this.MIN_USERNAME_LENGTH = authValidators.MIN_USERNAME_LENGTH;
    this.MAX_USERNAME_LENGTH = authValidators.MAX_USERNAME_LENGTH;
    this.MIN_PASSWORD_LENGTH = authValidators.MIN_PASSWORD_LENGTH;
    this.MAX_PASSWORD_LENGTH = authValidators.MAX_PASSWORD_LENGTH;
    this.router = router;

    this.usernameControl = new FormControl("", authValidators.buildUsernameValidatorsArray());
    this.emailControl = new FormControl("", [Validators.required, Validators.email]);
    this.passwordControl = new FormControl("",authValidators.buildPasswordValidatorsArray());
    this.passwordConfirmationControl = new FormControl("");
    this.rememberMeControl = new FormControl(true);

    this.formGroup = new FormGroup({
      usernameControl: this.usernameControl,
      emailControl: this.emailControl,
      passwordControl: this.passwordControl,
      passwordConfirmationControl: this.passwordConfirmationControl,
      rememberMeControl: this.rememberMeControl 
    }, [authValidators.buildPasswordsMatchValidator("passwordControl", "passwordConfirmationControl")]);
  }
  ngOnInit()
  {
    this.authStateSubscription = this.authManager.jwtSignedInUser$.subscribe({
      next: this.onUserSignedIn.bind(this)
    });
  }
  ngOnDestroy()
  {
      this.authStateSubscription?.unsubscribe();
    this.authStateSubscription = null;
    this.signUpSubscription?.unsubscribe();
    this.signUpSubscription = null;
  }
  getUsernameRequiredError()
  {
    return this.usernameControl.getError("required") != null? "Username is required": null;
  }
  getUsernameLengthError()
  {
    if (this.usernameControl.getError("minlength") || this.usernameControl.getError("maxlength"))
      return `Must be ${this.MIN_USERNAME_LENGTH} - ${this.MAX_USERNAME_LENGTH} characters.`;
    return null;
  }
  getUsernameLetterError()
  {
    if (this.usernameControl.getError("letterRequiredValidator"))
      return this.usernameControl.getError("letterRequiredValidator");
    return null;
  }
  getUsernameSpecificError()
  {
    if (this.usernameControl.getError("usernameValidator"))
      return this.usernameControl.getError("usernameValidator");
    return null;
  }
  getEmailError()
  {
    if (this.emailControl.getError("required"))
      return "Email is required.";
    if (this.emailControl.getError("email"))
      return "Invalid email.";
    return null;
  }
  getPasswordRequiredError()
  {
    if (this.passwordControl.getError("required"))
      return "Password is required";
    return null;
  }
  getPasswordLengthError()
  {
    if (this.passwordControl.getError("minlength") || this.passwordControl.getError("maxlength"))
      return `Must be ${this.MIN_PASSWORD_LENGTH} - ${this.MAX_PASSWORD_LENGTH} characters.`;
    return null;
  }
  getPasswordLetterError()
  {
    if (this.passwordControl.getError("letterRequiredValidator"))
      return this.passwordControl.getError("letterRequiredValidator");
    return null;
  }
  getPasswordDigitError()
  {
    if (this.passwordControl.getError("digitRequiredValidator"))
      return this.passwordControl.getError("digitRequiredValidator");
    return null;
  }
  getPasswordsMatchError()
  {
    if (this.formGroup.getError("passwordsMatchValidator"))
      return this.formGroup.getError("passwordsMatchValidator");
    return null;
  }
  submit()
  {
    if (!this.formGroup.valid)
      return;
    this.authProvider.signUpData = new SignUpEmailPasswordDto(this.usernameControl.value, this.emailControl.value, this.passwordControl.value);
    this.signUpSubscription = this.authManager.signUp(this.authProvider, this.rememberMeControl.value).subscribe(
      {
        next: this.onSignUpResponse.bind(this),
        error: this.onServerError.bind(this)
      }
    );
  }
  getServerError()
  {
    if (this.formGroup.getError("serverError"))
      return this.formGroup.getError("serverError");
    return null;
  }
  onSignUpResponse()
  {
    this.signUpSubscription?.unsubscribe;
    this.signUpSubscription = null;
  }
  onServerError(error: HttpErrorResponse)
  {
    this.formGroup.setErrors({ serverError: error.error });
    this.onSignUpResponse();
  }
  private onUserSignedIn()
  {
    if (!this.redirectOnSignedIn)
      return;
    if (this.authManager.isSignedIn())
      this.router.navigate(["/"]);
  }
}
