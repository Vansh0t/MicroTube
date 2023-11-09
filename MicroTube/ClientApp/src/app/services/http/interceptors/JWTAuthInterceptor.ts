/* eslint-disable @typescript-eslint/no-explicit-any */
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { AuthManager } from "../../auth/AuthManager";

@Injectable()
export class JWTAuthInterceptor implements HttpInterceptor
{
  private readonly authManager: AuthManager;

  constructor(authManager: AuthManager)
  {
    this.authManager = authManager;
  }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>>
  {
    if (!this.authManager.isSignedIn())
      return next.handle(req);
    const authRequest = req.clone(
      {
        setHeaders: {
          Authorization: `Bearer ${this.authManager.jwtSignedInUser$.value?.jwt}`
        }
      });
    return next.handle(authRequest);
  }
}
