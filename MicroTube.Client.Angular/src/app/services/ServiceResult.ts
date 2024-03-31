
export class ServiceResult
{
  code: number;
  isError: boolean;
  error: string | null;
  constructor(code: number, isError: boolean, error: string | null)
  {
    this.code = code;
    this.isError = isError;
    this.error = error;
  }
}
export class ServiceResultObject<T> extends ServiceResult
{
  object: T;
  constructor(code: number, object : T, isError: boolean, error: string | null)
  {
    super(code, isError, error);
    this.object = object;
  }
}
