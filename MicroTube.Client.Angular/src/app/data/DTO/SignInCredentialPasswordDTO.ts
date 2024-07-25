
export class SignInCredentialPasswordDTO
{
  credential: string;
  password: string;
  constructor(credential: string, password:string)
  {
    this.credential = credential;
    this.password = password;
  }
}
