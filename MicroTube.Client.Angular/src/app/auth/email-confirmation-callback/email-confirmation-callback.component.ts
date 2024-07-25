import { Component, OnDestroy, OnInit } from "@angular/core";
import { EmailPasswordAuthProvider } from "../../services/auth/providers/EmailPasswordAuthProvider";
import { ActivatedRoute, Router } from "@angular/router";
import { AuthenticationResponseDTO } from "../../data/DTO/AuthenticationResponseDTO";
import { Subscription, timer } from "rxjs";
import { HttpErrorResponse } from "@angular/common/http";
import { AuthManager } from "../../services/auth/AuthManager";
import { RequestStatus } from "../../enums";

@Component({
  selector: "email-confirmation-callback",
  templateUrl: "./email-confirmation-callback.component.html",
  styleUrls: ["./email-confirmation-callback.component.css"]
})
export class EmailConfirmationCallbackComponent implements OnInit, OnDestroy
{
  readonly REDIRECT_TIME_DELAY_MS = 4000;

  private readonly authManager: AuthManager;
  private readonly authProvider: EmailPasswordAuthProvider;
  private readonly activatedRoute: ActivatedRoute;
  private readonly router: Router;
  private requestSubscription: Subscription | undefined;
  private timerSubscription: Subscription | undefined;

  RequestStatus: typeof RequestStatus = RequestStatus;
  status: RequestStatus = RequestStatus.NotStarted;
  constructor(authManager: AuthManager, authProvider: EmailPasswordAuthProvider, activatedRoute: ActivatedRoute, router: Router)
  {
    this.authManager = authManager;
    this.authProvider = authProvider;
    this.activatedRoute = activatedRoute;
    this.router = router;
  }
  ngOnInit(): void
  {
    const emailConfirmationString = this.activatedRoute.snapshot.queryParams["emailConfirmationString"];
    const requestObservable = this.authProvider.confirmEmail(emailConfirmationString);
    this.status = RequestStatus.InProgress;
    this.requestSubscription = requestObservable.subscribe({
      next: this.onConfirmationSuccess.bind(this),
      error: this.onConfirmationError.bind(this)
    });
  }
  ngOnDestroy(): void
  {
    this.requestSubscription?.unsubscribe();
    this.timerSubscription?.unsubscribe();
  }
  onConfirmationSuccess(authResponse: AuthenticationResponseDTO): void
  {
    if (authResponse != null && this.authManager.isSignedIn())
    {
      this.authManager.applyAuthResult(authResponse);
    }
    this.status = RequestStatus.Success;
    const timerObservable = timer(4000);
    this.timerSubscription = timerObservable.subscribe(() => this.redirectToMain());
   
  }
  onConfirmationError(error: HttpErrorResponse): void
  {
    console.error(error);
    this.status = RequestStatus.Error;
  }
  redirectToMain()
  {
    this.router.navigate(["/"]);
  }
}
