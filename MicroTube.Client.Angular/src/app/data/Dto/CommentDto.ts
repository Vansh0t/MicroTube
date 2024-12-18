import { DateTime } from "luxon";
import { LikeDislikeReactionDto } from "./LikeDislikeReactionDto";
import { LikeDislikeReactionsAggregationDto } from "./LikeDislikeReactionsAggregationDto";
export class CommentDto
{
  id: string;
  userId: string;
  userAlias: string | null;
  videoId: string;
  content: string;
  time: DateTime;
  reaction: LikeDislikeReactionDto | null;
  reactionsAggregation: LikeDislikeReactionsAggregationDto | null;
  deleted: boolean = false;
  edited: boolean;
  constructor(raw: CommentRawDto)
  {
    this.id = raw.id;
    this.userId = raw.userId;
    this.userAlias = raw.userAlias;
    this.videoId = raw.videoId;
    this.content = raw.content;
    this.time = DateTime.fromISO(raw.time, { zone: "utc" });
    this.reaction = raw.reaction;
    this.reactionsAggregation = raw.reactionsAggregation;
    this.edited = raw.edited;
  }
  belongsToUser(userId: string | null)
  {
    if (userId == null)
    {
      return false;
    }
    return this.userId === userId;
  }
}
export interface CommentRawDto
{
  id: string;
  userId: string;
  userAlias: string | null;
  videoId: string;
  content: string;
  time: string;
  edited: boolean;
  reaction: LikeDislikeReactionDto | null;
  reactionsAggregation: LikeDislikeReactionsAggregationDto | null;
}
