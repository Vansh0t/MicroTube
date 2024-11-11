import { LikeDislikeReactionType } from "../../services/ReactionTypes";

export interface LikeDislikeReactionDto
{
  userId: string;
  commentId: string;
  reactionType: LikeDislikeReactionType;
}
