import { Injectable } from "@angular/core";
import { JWTUser } from "./JWTUser";
import { IAuthProvider } from "./providers/IAuthProvider";
import { BehaviorSubject} from "rxjs";
import { AuthenticationResponseDTO } from "../../data/DTO/AuthenticationResponseDTO";
import { HttpErrorResponse } from "@angular/common/http";

@Injectable({
  providedIn: "root"
})
export class AuthManager
{
  private readonly JWT_STORAGE_KEY = "jwt";
  private readonly REMEMBER_USER_STORAGE_KEY = "rememberUser";

  readonly jwtSignedInUser$: BehaviorSubject<JWTUser | null>;
  get rememberUser(): boolean
  {
    const value = localStorage.getItem(this.REMEMBER_USER_STORAGE_KEY);
    return value != null ? true : false;
  }
  set rememberUser(value: boolean)
  {
    if (value)
      localStorage.setItem(this.REMEMBER_USER_STORAGE_KEY, "true");
    else
      localStorage.removeItem(this.REMEMBER_USER_STORAGE_KEY);
  }
  constructor()
  {
    const storageJWT = this.rememberUser ? this.readJWTFromLocalStorage() : this.readJWTFromSessionStorage();
    if (storageJWT == null)
    {
      this.jwtSignedInUser$ = new BehaviorSubject<JWTUser | null>(null);
      return;
    }
    try
    {
      const jwtUser = new JWTUser(storageJWT);
      if (jwtUser.isExpired())
      {
        //refresh jwt here;
      }
      this.jwtSignedInUser$ = new BehaviorSubject<JWTUser | null>(jwtUser);
    }
    catch
    {
      this.jwtSignedInUser$ = new BehaviorSubject<JWTUser | null>(null);
    }
  }
  signOut()
  {
    this.rememberUser = false;
    this.clearJWTStorage();
    this.jwtSignedInUser$.next(null);
  }
  signUp(authProvider: IAuthProvider, rememberUser: boolean, onError: (error: HttpErrorResponse) => void)
  {
    authProvider.signUp()
      .subscribe({
        next: this.onSignInResult.bind(this),
        error: onError
      });
    this.rememberUser = rememberUser;
  }
  signIn(authProvider: IAuthProvider, rememberUser: boolean, onError: (error: HttpErrorResponse) => void)
  {
    authProvider.signIn()
      .subscribe({
        next: this.onSignInResult.bind(this),
        error: onError
      });
    this.rememberUser = rememberUser;
  }
  isSignedIn(): boolean
  {
    if (this.jwtSignedInUser$.value == null)
      return false;
    if (this.jwtSignedInUser$.value.isExpired())
    {
      this.jwtSignedInUser$.next(null);
      return false;
    }
    return true;
  }
  private onSignInResult(response: AuthenticationResponseDTO)
  {
    if (response == null || response.jwt == null || response.jwt.trim() == "")
      throw new Error("Got invalid sign in response. The response or response.jwt is null");
    const jwtUser = new JWTUser(response.jwt);
    this.storeJWT(response.jwt);
    this.jwtSignedInUser$.next(jwtUser);
  }
  private readJWTFromLocalStorage(): string | null
  {
    return localStorage.getItem(this.JWT_STORAGE_KEY);
  }
  private readJWTFromSessionStorage(): string | null
  {
    return sessionStorage.getItem(this.JWT_STORAGE_KEY);
  }
  private storeJWT(jwt: string): void
  {
    if (jwt == null || jwt.length <= 0)
    {
      throw new Error("Got invalid jwt string for storage");
    }
    if (this.rememberUser)
    {
      sessionStorage.removeItem(this.JWT_STORAGE_KEY);
      localStorage.setItem(this.JWT_STORAGE_KEY, jwt);
    }
    else
    {
      localStorage.removeItem(this.JWT_STORAGE_KEY);
      sessionStorage.setItem(this.JWT_STORAGE_KEY, jwt);
    }
  }
  private clearJWTStorage()
  {
    localStorage.removeItem(this.JWT_STORAGE_KEY);
    sessionStorage.removeItem(this.JWT_STORAGE_KEY);
  }
}
