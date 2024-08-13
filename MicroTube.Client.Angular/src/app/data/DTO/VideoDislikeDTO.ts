import { DateTime } from "luxon";

export interface VideoDislikeDTO
{
  id: string;
  videoId: string;
  userId: string;
  time: DateTime;
}
