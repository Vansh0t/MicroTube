
export class AuthenticationResponseDTO
{
  jwt: string;
  constructor(jwt: string)
  {
    this.jwt = jwt;
  }
}
