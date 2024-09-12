import { DateTime } from "luxon";
import { VideoReactionType } from "../../services/videos/VideoService";
export interface UserVideoReactionDTO
{
  userId: string;
  videoId: string;
  time: DateTime;
  reactionType: VideoReactionType;
}
