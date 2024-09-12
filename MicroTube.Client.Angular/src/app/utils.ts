export function parseBoolean(value: string): boolean
{
  if (value == null)
    throw new Error("value is null");
  try
  {
    return JSON.parse(value.toLowerCase());
  }
  catch {
    throw new Error("Unable to parse: value is not a boolean");
  }
}
