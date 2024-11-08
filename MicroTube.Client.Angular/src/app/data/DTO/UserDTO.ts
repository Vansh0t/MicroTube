
export class UserDto
{
  id: string;
  username: string;
  email: string;
  publicUsername: string;
  isEmailConfirmed: string;

  constructor(id: string, username: string, email: string, publicUsername: string, isEmailConfirmed: string)
  {
    this.id = id;
    this.username = username;
    this.email = email;
    this.publicUsername = publicUsername;
    this.isEmailConfirmed = isEmailConfirmed;
  }
}
