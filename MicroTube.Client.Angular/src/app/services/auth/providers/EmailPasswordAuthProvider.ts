import { Observable } from "rxjs";
import { AuthenticationResponseDto } from "../../../data/Dto/AuthenticationResponseDto";
import { SignInCredentialPasswordDto } from "../../../data/Dto/SignInCredentialPasswordDto";
import { IAuthProvider } from "./IAuthProvider";
import { HttpClient, HttpHeaders, HttpResponse } from "@angular/common/http";
import { Inject, Injectable } from "@angular/core";
import { SignUpEmailPasswordDto } from "../../../data/Dto/SignUpEmailPasswordDto";
import { ResetPasswordDto } from "../../../data/Dto/ResetPasswordDto";
import { MessageDto } from "../../../data/Dto/MessageDto";
import { PasswordChangeDto } from "../../../data/Dto/PasswordChangeDto";
import { EmailChangeDto } from "../../../data/Dto/EmailChangeDto";
import { PasswordResetTokenDto } from "../../../data/Dto/PasswordResetTokenDto";
@Injectable({
  providedIn:"root"
})
export class EmailPasswordAuthProvider implements IAuthProvider
{
  private readonly client: HttpClient;

  signInData: SignInCredentialPasswordDto | undefined = undefined;
  signUpData: SignUpEmailPasswordDto | undefined = undefined;
  constructor(client: HttpClient)
  {
    this.client = client;
  }
  signIn(): Observable<AuthenticationResponseDto>
  {
    if (this.signInData == undefined)
      throw new Error("signInData must be provided before sign in attempt");
    const result = this.client.post<AuthenticationResponseDto>("authentication/emailpassword/signin", this.signInData, { withCredentials: true });
    this.signInData = undefined;
    return result;
  }
  signUp(): Observable<AuthenticationResponseDto>
  {
    if (this.signUpData == undefined)
      throw new Error("signUpData must be provided before sign up attempt");
    const result = this.client.post<AuthenticationResponseDto>("authentication/emailpassword/signup", this.signUpData, { withCredentials: true });
    this.signUpData = undefined;
    return result;
  }
  confirmEmail(confirmationString: string): Observable<AuthenticationResponseDto>
  {
    if (confirmationString == null || confirmationString.trim() == "")
      throw new Error("invalid confirmation string");
    const result = this.client.post<AuthenticationResponseDto>("authentication/emailpassword/confirmemail", new MessageDto(confirmationString), { withCredentials: true });
    return result;
  }
  requestPasswordReset(email: string): Observable<MessageDto>
  {
    if (email == null || email.trim() == "")
      throw new Error("invalid email");
    const result = this.client.post<MessageDto>("authentication/emailpassword/resetpassword", new ResetPasswordDto(email));
    return result;
  }
  resendEmailConfirmation(): Observable<null> {
    const result = this.client.post<null>("authentication/emailpassword/confirmemailresend", null);
    return result;
  }
  getPasswordResetToken(resetString: string): Observable<PasswordResetTokenDto>
  {
    if (resetString == null || resetString.trim() == "")
      throw new Error("invalid reset string");
    const result = this.client.post<PasswordResetTokenDto>("authentication/emailpassword/validatepasswordreset", new MessageDto(resetString));
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
    const result = this.client.post<HttpResponse<null>>("authentication/emailpassword/changepassword", new PasswordChangeDto(newPassword), { headers: headers });
    return result;
  }
  startEmailChange(email: string, password: string): Observable<HttpResponse<null>>
  {
    if (email == null || email.trim() == "")
      throw new Error("invalid email");
    if (password == null || password.trim() == "")
      throw new Error("invalid password");
    const result = this.client.post<HttpResponse<null>>("authentication/emailpassword/changeemailstart", new EmailChangeDto(email, password));
    return result;
  }
}
