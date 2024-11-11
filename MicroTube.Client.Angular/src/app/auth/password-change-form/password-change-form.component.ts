import { Component, OnDestroy, OnInit } from "@angular/core";
import { FormControl, FormGroup } from "@angular/forms";
import { EmailPasswordAuthProvider } from "../../services/auth/providers/EmailPasswordAuthProvider";
import { DefaultAuthValidators } from "../../services/validation/DefaultAuthValidators";
import { ActivatedRoute } from "@angular/router";
import { Subscription } from "rxjs";
import { RequestStatus } from "../../enums";
import { HttpErrorResponse } from "@angular/common/http";
import { PasswordResetTokenDto } from "../../data/Dto/PasswordResetTokenDto";

@Component({
  selector: "password-change-form",
  templateUrl: "./password-change-form.component.html",
  styleUrls: ["./password-change-form.component.css"]
})
export class PasswordChangeFormComponent implements OnInit, OnDestroy
{
  readonly formGroup: FormGroup;
  readonly passwordControl: FormControl;
  readonly passwordConfirmationControl: FormControl;

  showForm: boolean = false;
  RequestStatus: typeof RequestStatus = RequestStatus;
  status: RequestStatus = RequestStatus.NotStarted;


  private readonly MIN_PASSWORD_LENGTH: number;
  private readonly MAX_PASSWORD_LENGTH: number;
  private readonly authProvider: EmailPasswordAuthProvider;
  private readonly activatedRoute: ActivatedRoute;
  private resetJWT: string | null = null;

  private requestSubscription: Subscription | null = null;
  constructor(authProvider: EmailPasswordAuthProvider, authValidators: DefaultAuthValidators, activatedRoute: ActivatedRoute)
  {
    this.MIN_PASSWORD_LENGTH = authValidators.MIN_PASSWORD_LENGTH;
    this.MAX_PASSWORD_LENGTH = authValidators.MAX_PASSWORD_LENGTH;
    this.authProvider = authProvider;
    this.passwordControl = new FormControl("", authValidators.buildPasswordValidatorsArray());
    this.passwordConfirmationControl = new FormControl("");
    this.formGroup = new FormGroup(
      {
        passwordControl: this.passwordControl,
        passwordConfirmationControl: this.passwordConfirmationControl
      },
      [authValidators.buildPasswordsMatchValidator("passwordControl", "passwordConfirmationControl")]);
    this.activatedRoute = activatedRoute;
  }
  ngOnInit()
  {
    try
    {
      const getPasswordRequest = this.authProvider.getPasswordResetToken(this.activatedRoute.snapshot.queryParams["passwordResetString"]);
      this.requestSubscription = getPasswordRequest.subscribe({
        next: this.onGetResetJWT.bind(this),
        error: (error: HttpErrorResponse) =>
        {
          console.error(error.error);
          this.status = RequestStatus.Error;
        }
      });
      this.status = RequestStatus.InProgress;
    }
    catch (e)
    {
      console.error(e);
      this.status = RequestStatus.Error;
    }
  }
  ngOnDestroy()
  {
    this.requestSubscription?.unsubscribe();
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
    if (this.formGroup.valid && this.showForm)
    {
      try
      {
        const passwordChangeRequest = this.authProvider.changePassword(<string>this.resetJWT, this.passwordControl.value);
        this.status = RequestStatus.InProgress;
        this.requestSubscription = passwordChangeRequest.subscribe({
          next: this.onPasswordChanged.bind(this),
          error: (error: HttpErrorResponse) =>
          {
            console.error(error.error);
            this.status = RequestStatus.Error;
          }
        });
      }
      catch (e)
      {
        console.error(e);
        this.status = RequestStatus.Error;
      }
      
    }
  }
  private onGetResetJWT(authResponse: PasswordResetTokenDto): void
  {
    this.status = RequestStatus.Success;
    this.showForm = true;
    this.resetJWT = authResponse.jwt;
    this.status = RequestStatus.NotStarted;
  }
  private onPasswordChanged(): void
  {
    this.status = RequestStatus.Success;
  }
}
