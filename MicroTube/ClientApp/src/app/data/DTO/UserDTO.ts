
export class UserDTO
{
  id: number;
  username: string;
  email: string;
  publicUsername: string;
  isEmailConfirmed: string;

  constructor(id: number, username: string, email: string, publicUsername: string, isEmailConfirmed: string)
  {
    this.id = id;
    this.username = username;
    this.email = email;
    this.publicUsername = publicUsername;
    this.isEmailConfirmed = isEmailConfirmed;
  }
}
