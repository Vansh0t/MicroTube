import { Injectable } from "@angular/core";
import { DateTime } from "luxon";


@Injectable({
  providedIn: "root"
})
export class TimeFormatter
{
  getUserFriendlyTimeDifference(from: DateTime, to: DateTime): string
  {
    const timeDifference = to.diff(from);
    const timeDifferenceHours = timeDifference.as("hours");
    if (timeDifferenceHours < 1)
    {
      return "less than hour ago";
    }
    if (timeDifferenceHours < 24)
    {
      return Math.round(timeDifferenceHours) + " hours ago";
    }
    const timeDifferenceDays = timeDifference.as("days");
    if (timeDifferenceDays < 2)
    {
      return "1 day ago";
    }
    if (timeDifferenceDays < 31)
    {
      return Math.round(timeDifferenceDays) + " days ago";
    }
    const timeDifferenceMonths = timeDifference.as("months");
    if (timeDifferenceMonths < 2)
    {
      return "1 month ago";
    }
    if (timeDifferenceMonths < 12)
    {
      return Math.round(timeDifferenceMonths) + " months ago";
    }
    const timeDifferenceYears = timeDifference.as("years");
    return timeDifferenceYears < 2 ? "1 year ago" : Math.round(timeDifferenceYears) + " years ago";
  }
}
