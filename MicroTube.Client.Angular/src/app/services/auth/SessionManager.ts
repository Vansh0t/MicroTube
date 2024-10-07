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
    this.authManager = authManager;
    this.client = client;
    this.authManager.jwtSignedInUser$.subscribe({
      next: this.onAccessTokenChanged.bind(this)
    });
  }
  private onAccessTokenChanged(jwtUser: JWTUser | null)
  {
    if (jwtUser == null || !this.authManager.rememberUser)
    {
      this.nextRefresh?.unsubscribe();
      return;
    }
    const timeDiff = jwtUser.expirationTime.diff(DateTime.utc()).toMillis() - this.REFRESH_PREFIRE_MS;
    if (jwtUser.isExpired() || timeDiff <= 0)
    {
      this.refreshSession();
      return;
    }
    this.nextRefresh = timer(timeDiff)
      .subscribe({
        next: this.refreshSession.bind(this)
      });
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
    this.authManager.applyAuthResult(response);
  }
  private onRefreshFail(errorResponse: HttpErrorResponse)
  {
    console.error("Failed to refresh user session: " + errorResponse);
    //if we failed to refresh a token, consider session invalidated and sign out immediately
    this.authManager.signOut();
  }
}
