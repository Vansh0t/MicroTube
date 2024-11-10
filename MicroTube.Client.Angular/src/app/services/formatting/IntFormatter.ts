import { Injectable } from "@angular/core";


@Injectable({
  providedIn: "root"
})
export class IntFormatter
{
  getUserFriendlyInt(input: number): string
  {
    if (input < 1000)
    {
      return input.toString();
    }
    if (input < 1000000)
    {
      return Math.round(input / 1000) + "K";
    }
    return (input / 1000000).toFixed(2) + "M";
  }
}
