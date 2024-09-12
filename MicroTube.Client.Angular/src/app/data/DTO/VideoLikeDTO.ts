import { DateTime } from "luxon";

export interface VideoLikeDTO
{
  id: string;
  videoId: string;
  userId: string;
  time: DateTime;
}
