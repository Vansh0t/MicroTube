import { Component, OnDestroy, OnInit } from "@angular/core";
import { FormControl, FormGroup, Validators } from "@angular/forms";
import { AuthManager } from "../../services/auth/AuthManager";
import { Router } from "@angular/router";
import { UserDto } from "../../data/Dto/UserDto";
import { Observable, Subscription, catchError, tap, throwError } from "rxjs";
import { ProfileManager } from "../../services/user/ProfileManager";
import { HttpErrorResponse } from "@angular/common/http";
import { EmailPasswordAuthProvider } from "../../services/auth/providers/EmailPasswordAuthProvider";
import { MessageDto } from "../../data/Dto/MessageDto";
import { InfoPopupDialogComponent } from "../../utility-components/info-popup-dialog/info-popup-dialog.component";
import { MatDialog } from "@angular/material/dialog";
import { JWTUser } from "../../services/auth/JWTUser";

@Component({
  selector: "email-password-profile",
  templateUrl: "./email-password-profile.component.html",
  styleUrls: ["./email-password-profile.component.css"]
})
export class EmailPasswordProfileComponent implements OnInit, OnDestroy
{
  readonly emailChangeGroup: FormGroup;
  readonly emailChangeNewEmailControl: FormControl;
  readonly emailChangePasswordControl: FormControl;


  user$: Observable<UserDto> | null = null;
  user: UserDto | null = null;
  userRequestError: string | null = null;

  readonly authManager: AuthManager;
  readonly authProvider: EmailPasswordAuthProvider;
  private readonly profileManager: ProfileManager;
  private readonly dialog: MatDialog;
  private readonly router: Router;
  private readonly signInSubscription: Subscription;

  constructor(authManager: AuthManager, profileManager: ProfileManager, authProvider: EmailPasswordAuthProvider, router: Router, dialog: MatDialog)
  {
    this.router = router;
    this.dialog = dialog;
    this.authProvider = authProvider;
    this.profileManager = profileManager;
    this.authManager = authManager;
    this.emailChangeNewEmailControl = new FormControl("", [Validators.required, Validators.email]);
    this.emailChangePasswordControl = new FormControl("", [Validators.required]);
    this.emailChangeGroup = new FormGroup({
      emailChangeNewEmailControl: this.emailChangeNewEmailControl,
      emailChangePasswordControl: this.emailChangePasswordControl
    });
    this.signInSubscription = this.authManager.jwtSignedInUser$.subscribe({
      next: this.onSignInStatusChanged.bind(this)
    });
    
    
  }
  ngOnInit(): void {
    this.user$ = this.profileManager.getUser().pipe(
      tap((response: UserDto) =>
      {
        this.user = response;
      },
      catchError((error: HttpErrorResponse) =>
      {
        this.userRequestError = error.status + ": " + error.error;
        return throwError(() => error);
      })));
  }
  ngOnDestroy(): void
  {
    this.signInSubscription.unsubscribe();
  }
  submitEmailChange() : void
  {
    if (this.emailChangeGroup.valid)
    {
      this.authProvider.startEmailChange(this.emailChangeNewEmailControl.value, this.emailChangePasswordControl.value)
        .subscribe({
          next: this.onEmailChangeResponse.bind(this),
          error: (error: HttpErrorResponse) =>
          {
            if (error.status == 403)
            {
              this.emailChangePasswordControl.setErrors({ serverError: error.error });
            }
            else
            {
              this.emailChangeNewEmailControl.setErrors({serverError: error.error});
            }
          }
        });
    }
  }
  submitPasswordReset(): void
  {
    if (this.canChangePassword())
    {
    
      this.authProvider.requestPasswordReset(this.user!.email)
        .subscribe({
          next: this.onPasswordResetResponse.bind(this),
          error: (error) => console.error(error)
        });
    }
  }
  canChangePassword(): boolean
  {
    if (this.authManager.jwtSignedInUser$.value == null || this.user == null)
      return false;
    return this.authManager.jwtSignedInUser$.value.isEmailConfirmed;
  }
  getEmailError()
  {
    if (this.emailChangeNewEmailControl.getError("required"))
      return "Email is required.";
    if (this.emailChangeNewEmailControl.getError("email"))
      return "Invalid email.";
    return null;
  }
  getPasswordError()
  {
    if (this.emailChangePasswordControl.getError("required"))
      return "Password is required.";
    return null;
  }
  getEmailServerError()
  {
    if (this.emailChangeNewEmailControl.getError("serverError"))
      return this.emailChangeNewEmailControl.getError("serverError");
    return null;
  }
  getEmailChangePasswordServerError()
  {
    if (this.emailChangePasswordControl.getError("serverError"))
      return this.emailChangePasswordControl.getError("serverError");
    return null;
  }

  private onPasswordResetResponse(response: MessageDto)
  {
    console.log(response);
    if (this?.dialog != null)
      this.dialog.open(InfoPopupDialogComponent, {
        data:
        {
          info: "Password change instructions were sent to your email."
        }
      });
  }
  private onEmailChangeResponse(response: object)
  {
    console.log(response);
    if (this?.dialog != null)
      this.dialog.open(InfoPopupDialogComponent, {
        data:
        {
          info: "An email was sent to the new address. Please, use it to verify the address ownership."
        }
      });
  }
  private onSignInStatusChanged(jwtUserValue: JWTUser | null)
  {
    if (jwtUserValue == null)
    {
      this.router.navigate(["/"]);
      return;
    }
  }
}
