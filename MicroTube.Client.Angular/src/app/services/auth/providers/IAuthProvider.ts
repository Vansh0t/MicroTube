import { Observable } from "rxjs";
import { AuthenticationResponseDto } from "../../../data/Dto/AuthenticationResponseDto";

export interface IAuthProvider
{
  signIn(): Observable<AuthenticationResponseDto>;
  signUp(): Observable<AuthenticationResponseDto>;
}
