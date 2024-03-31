import { Observable } from "rxjs";
import { AuthenticationResponseDTO } from "../../../data/DTO/AuthenticationResponseDTO";

export interface IAuthProvider
{
  signIn(): Observable<AuthenticationResponseDTO>;
  signUp(): Observable<AuthenticationResponseDTO>;
}
