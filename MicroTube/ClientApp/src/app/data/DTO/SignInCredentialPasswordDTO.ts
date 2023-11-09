
export class SignInCredentialPasswordDTO
{
  Credential: string;
  Password: string;
  constructor(credential: string, password:string)
  {
    this.Credential = credential;
    this.Password = password;
  }
}
