import { DateTime } from "luxon";
import { InvalidTokenError, JwtPayload, jwtDecode} from "jwt-decode";
import { parseBoolean } from "../../utils";
export class JWTUser
{
  readonly userId: number;
  readonly publicUsername: string;
  readonly isEmailConfirmed: boolean;
  readonly issuedTime: DateTime;
  readonly expirationTime: DateTime;
  readonly jwt: string;

  private readonly passwordReset: boolean = false;
  private readonly user: boolean = false;

  constructor(jwtToken: string)
  {
    const decodeResult = jwtDecode<JWTUserPayload>(jwtToken);
    this.validateJWTPayload(decodeResult);
    this.userId = parseInt(<string>decodeResult.sub);
    this.expirationTime = DateTime.fromSeconds(<number>decodeResult.exp);
    this.issuedTime = DateTime.fromSeconds(<number>decodeResult.nbf);
    this.isEmailConfirmed = parseBoolean(<string>decodeResult.email_confirmed);
    this.publicUsername = <string>decodeResult.public_name;
    this.jwt = jwtToken;

    if (decodeResult.user != null)
      this.user = parseBoolean(<string>decodeResult.user);
    if (decodeResult.password_reset != null)
      this.passwordReset = parseBoolean(<string>decodeResult.password_reset);
  }

  isExpired(): boolean
  {
    const currentDateTime = DateTime.utc();
    return currentDateTime > this.expirationTime;
  }
  canChangePassword()
  {
    return this.passwordReset;
  }
  isUser()
  {
    return this.user;
  }
  private validateJWTPayload(payload: JWTUserPayload)
  {
    if (payload.sub == undefined)
      throw new InvalidTokenError("sub claim is undefined");
    if (payload.exp == undefined)
      throw new InvalidTokenError("exp claim is undefined");
    if (payload.email_confirmed == undefined)
      throw new InvalidTokenError("email_confirmed claim is undefined");
    if (payload.public_name == undefined)
      throw new InvalidTokenError("public_name claim is undefined");
  }
}
export interface JWTUserPayload extends JwtPayload
{
  user: string | null;
  password_reset: string | null;
  email_confirmed: string | undefined;
  public_name: string | undefined;
}
