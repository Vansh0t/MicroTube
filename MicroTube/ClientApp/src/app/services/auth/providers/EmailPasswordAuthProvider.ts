import { Observable } from "rxjs";
import { AuthenticationResponseDTO } from "../../../data/DTO/AuthenticationResponseDTO";
import { SignInCredentialPasswordDTO } from "../../../data/DTO/SignInCredentialPasswordDTO";
import { IAuthProvider } from "./IAuthProvider";
import { HttpClient, HttpHeaders, HttpResponse } from "@angular/common/http";
import { Inject, Injectable } from "@angular/core";
import { SignUpEmailPasswordDTO } from "../../../data/DTO/SignUpEmailPasswordDTO";
import { ResetPasswordDTO } from "../../../data/DTO/ResetPasswordDTO";
import { MessageDTO } from "../../../data/DTO/MessageDTO";
import { PasswordChangeDTO } from "../../../data/DTO/PasswordChangeDTO";
import { EmailChangeDTO } from "../../../data/DTO/EmailChangeDTO";
import { PasswordResetTokenDTO } from "../../../data/DTO/PasswordResetTokenDTO";
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
    const result = this.client.post<AuthenticationResponseDTO>("Authentication/EmailPassword/SignIn", this.signInData, { withCredentials: true });
    this.signInData = undefined;
    return result;
  }
  signUp(): Observable<AuthenticationResponseDTO>
  {
    if (this.signUpData == undefined)
      throw new Error("signUpData must be provided before sign up attempt");
    const result = this.client.post<AuthenticationResponseDTO>("Authentication/EmailPassword/SignUp", this.signUpData, { withCredentials: true });
    this.signUpData = undefined;
    return result;
  }
  confirmEmail(confirmationString: string): Observable<AuthenticationResponseDTO>
  {
    if (confirmationString == null || confirmationString.trim() == "")
      throw new Error("invalid confirmation string");
    const result = this.client.post<AuthenticationResponseDTO>("Authentication/EmailPassword/ConfirmEmail", new MessageDTO(confirmationString), { withCredentials: true });
    return result;
  }
  requestPasswordReset(email: string): Observable<MessageDTO>
  {
    if (email == null || email.trim() == "")
      throw new Error("invalid email");
    const result = this.client.post<MessageDTO>("Authentication/EmailPassword/ResetPassword", new ResetPasswordDTO(email));
    return result;
  }
  resendEmailConfirmation(): Observable<null> {
    const result = this.client.post<null>("Authentication/EmailPassword/ConfirmEmailResend", null);
    return result;
  }
  getPasswordResetToken(resetString: string): Observable<PasswordResetTokenDTO>
  {
    if (resetString == null || resetString.trim() == "")
      throw new Error("invalid reset string");
    const result = this.client.post<PasswordResetTokenDTO>("Authentication/EmailPassword/ValidatePasswordReset", new MessageDTO(resetString));
    return result;
  }
  changePassword(passwordResetJWT: string, newPassword: string): Observable<HttpResponse<null>>
  {
    if (passwordResetJWT == null || passwordResetJWT.trim() == "")
      throw new Error("invalid passwordResetJWT");
    if (newPassword == null || newPassword.trim() == "")
      throw new Error("invalid newPassword");
    const headers = new HttpHeaders({
      Authorization: `Bearer ${passwordResetJWT}`
    });
    const result = this.client.post<HttpResponse<null>>("Authentication/EmailPassword/ChangePassword", new PasswordChangeDTO(newPassword), { headers: headers });
    return result;
  }
  startEmailChange(email: string, password: string): Observable<HttpResponse<null>>
  {
    if (email == null || email.trim() == "")
      throw new Error("invalid email");
    if (password == null || password.trim() == "")
      throw new Error("invalid password");
    const result = this.client.post<HttpResponse<null>>("Authentication/EmailPassword/ChangeEmailStart", new EmailChangeDTO(email, password));
    return result;
  }
}
