export class EmailChangeDTO
{
  newEmail: string;
  password: string;

  constructor(newEmail: string, password: string)
  {
    this.newEmail = newEmail;
    this.password = password;
  }
}
