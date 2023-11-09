
export class SignUpEmailPasswordDTO
{
  Username: string;
  Email: string;
  Password: string;
  constructor(username: string, email: string, password: string)
  {
    this.Username = username;
    this.Email = email;
    this.Password = password;
  }
}
