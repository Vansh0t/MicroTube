import { Injectable } from "@angular/core";
import { LikeDislikeReactionType } from "../ReactionTypes";
import { LikeDislikeReactionsAggregationDto } from "../../data/Dto/LikeDislikeReactionsAggregationDto";


@Injectable({
  providedIn: "root"
})
export class FakeLikeDislikeReactionsAggregator
{
  aggregate(aggregation: LikeDislikeReactionsAggregationDto, prevReaction: LikeDislikeReactionType, newReaction: LikeDislikeReactionType): LikeDislikeReactionsAggregationDto
  {
    if (prevReaction == newReaction)
    {
      return aggregation;
    }
    if (prevReaction == LikeDislikeReactionType.None)
    {
      if (newReaction == LikeDislikeReactionType.Like)
        aggregation.likes++;
      else if (newReaction == LikeDislikeReactionType.Dislike)
        aggregation.dislikes++;
    }
    else if (prevReaction == LikeDislikeReactionType.Like)
    {
      if (newReaction == LikeDislikeReactionType.Dislike)
      {
        aggregation.likes--;
        aggregation.dislikes++;
      }
      else if (newReaction == LikeDislikeReactionType.None)
      {
        aggregation.likes--;
      }
    }
    else if (prevReaction == LikeDislikeReactionType.Dislike)
    {
      if (newReaction == LikeDislikeReactionType.Like)
      {
        aggregation.likes++;
        aggregation.dislikes--;
      }
      else if (newReaction == LikeDislikeReactionType.None)
      {
        aggregation.dislikes--;
      }
    }
    return aggregation;
  }
}
