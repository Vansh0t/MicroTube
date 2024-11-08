import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { CommentReactionDto } from "../../data/Dto/CommentReactionDto";
import { LikeDislikeReactionType } from "../ReactionTypes";


@Injectable({
  providedIn: "root"
})
export class VideoCommentReactionService
{
  private readonly client: HttpClient;
  constructor(client: HttpClient)
  {
    this.client = client;
  }
  setReaction(commentId: string, reactionType: LikeDislikeReactionType): Observable<CommentReactionDto>
  {
    return this.client.post<CommentReactionDto>(`reactions/comment/${commentId}/react/${reactionType}`, {});
  }
  getReaction(commentId: string): Observable<CommentReactionDto>
  {
    return this.client.get<CommentReactionDto>(`reactions/comment/${commentId}/reaction`);
  }
}
