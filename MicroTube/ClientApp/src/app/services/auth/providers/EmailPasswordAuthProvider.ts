import { Observable } from "rxjs";
import { AuthenticationResponseDTO } from "../../../data/DTO/AuthenticationResponseDTO";
import { SignInCredentialPasswordDTO } from "../../../data/DTO/SignInCredentialPasswordDTO";
import { IAuthProvider } from "./IAuthProvider";
import { HttpClient } from "@angular/common/http";
import { Inject, Injectable } from "@angular/core";
import { SignUpEmailPasswordDTO } from "../../../data/DTO/SignUpEmailPasswordDTO";
@Injectable({
  providedIn:"root"
})
export class EmailPasswordAuthProvider implements IAuthProvider
{
  private readonly client: HttpClient;
  private readonly BASE_URL: string;

  signInData: SignInCredentialPasswordDTO | undefined = undefined;
  signUpData: SignUpEmailPasswordDTO | undefined = undefined;
  constructor(client: HttpClient, @Inject("BASE_URL") baseUrl: string)
  {
    this.client = client;
    this.BASE_URL = baseUrl;
  }
  signIn(): Observable<AuthenticationResponseDTO>
  {
    if (this.signInData == undefined)
      throw new Error("signInData must be provided before sign in attempt");
    const result = this.client.post<AuthenticationResponseDTO>("Authentication/EmailPassword/SignIn", this.signInData);
    this.signInData = undefined;
    return result;
  }
  signUp(): Observable<AuthenticationResponseDTO>
  {
    if (this.signUpData == undefined)
      throw new Error("signUpData must be provided before sign up attempt");
    const result = this.client.post<AuthenticationResponseDTO>("Authentication/EmailPassword/SignUp", this.signUpData);
    this.signUpData = undefined;
    return result;
  }
}
