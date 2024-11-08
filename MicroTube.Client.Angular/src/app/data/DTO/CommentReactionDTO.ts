import { LikeDislikeReactionType } from "../../services/ReactionTypes";

export interface CommentReactionDto
{
  userId: string;
  commentId: string;
  reactionType: LikeDislikeReactionType;
}
