import { Injectable } from "@angular/core";
import { AuthManager } from "./AuthManager";
import { JWTUser } from "./JWTUser";
import { Observable, Subscription, timer } from "rxjs";
import { AuthenticationResponseDTO } from "../../data/DTO/AuthenticationResponseDTO";
import { HttpClient, HttpErrorResponse } from "@angular/common/http";
import { DateTime } from "luxon";

@Injectable({
  providedIn: "root"
})
export class SessionManager
{
  private readonly REFRESH_PREFIRE_MS: number = 600000; //10 mins

  private readonly authManager: AuthManager;
  private readonly client: HttpClient;
  private nextRefresh: Subscription | null = null;
  constructor(authManager: AuthManager, client: HttpClient)
  {
    console.log("Session!");
    this.authManager = authManager;
    this.client = client;
    this.authManager.jwtSignedInUser$.subscribe({
      next: this.onAccessTokenChanged.bind(this)
    });
    console.log("session manager init");
  }
  private onAccessTokenChanged(jwtUser: JWTUser | null)
  {
    if (jwtUser == null || !this.authManager.rememberUser)
    {
      console.log(jwtUser);
      console.log(this.authManager.jwtSignedInUser$.value);
      console.log(this.authManager.rememberUser);
      this.nextRefresh?.unsubscribe();
      console.log("not signed in or chose not to be remembered, ignoring session refresh");
      return;
    }
    const timeDiff = jwtUser.expirationTime.diff(DateTime.utc()).toMillis() - this.REFRESH_PREFIRE_MS;
    if (jwtUser.isExpired() || timeDiff <= 0)
    {
      this.refreshSession();
      console.log("session is expired, refresh now");
      return;
    }
    this.nextRefresh = timer(timeDiff)
      .subscribe({
        next: this.refreshSession.bind(this)
      });
    console.log("access token changed, next update in " + timeDiff + " ms.");
  }
  refreshSession(): Observable<AuthenticationResponseDTO>
  {
    this.nextRefresh?.unsubscribe();
    const httpOptions = {
      withCredentials: true,
    };
    const request = this.client.post<AuthenticationResponseDTO>("authentication/session/refresh", null, httpOptions);
    request.subscribe({
      next: this.onRefreshSuccess.bind(this),
      error: this.onRefreshFail.bind(this)
    });
    return request;
  }
  private onRefreshSuccess(response: AuthenticationResponseDTO)
  {
    console.log("refreshed access " + response);
    this.authManager.applyAuthResult(response);
  }
  private onRefreshFail(errorResponse: HttpErrorResponse)
  {
    console.error("Failed to refresh: " + errorResponse);
    //if we failed to refresh a token, consider session invalidated and sign out immediately
    this.authManager.signOut();
  }
}
