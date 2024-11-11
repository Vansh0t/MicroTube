
export class PasswordChangeDto
{
  newPassword: string;
  constructor(newPassword: string)
  {
    this.newPassword = newPassword;
  }
}
