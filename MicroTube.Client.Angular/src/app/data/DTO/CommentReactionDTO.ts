import { LikeDislikeReactionType } from "../../services/ReactionTypes";

export interface CommentReactionDTO
{
  userId: string;
  commentId: string;
  reactionType: LikeDislikeReactionType;
}
