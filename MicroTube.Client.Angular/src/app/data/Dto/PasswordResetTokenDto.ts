
export class PasswordResetTokenDto
{
  jwt: string;
  constructor(jwt: string)
  {
    this.jwt = jwt;
  }
}
