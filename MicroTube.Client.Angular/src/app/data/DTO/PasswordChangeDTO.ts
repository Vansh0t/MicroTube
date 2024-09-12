
export class PasswordChangeDTO
{
  newPassword: string;
  constructor(newPassword: string)
  {
    this.newPassword = newPassword;
  }
}
