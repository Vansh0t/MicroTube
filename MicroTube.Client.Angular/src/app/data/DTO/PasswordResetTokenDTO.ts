
export class PasswordResetTokenDTO
{
  jwt: string;
  constructor(jwt: string)
  {
    this.jwt = jwt;
  }
}
