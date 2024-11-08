import { DateTime } from "luxon";
import { LikeDislikeReactionType } from "../../services/ReactionTypes";
export interface UserVideoReactionDto
{
  userId: string;
  videoId: string;
  time: DateTime;
  reactionType: LikeDislikeReactionType;
}
