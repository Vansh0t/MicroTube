import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { CommentReactionDTO } from "../../data/DTO/CommentReactionDTO";
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
  setReaction(commentId: string, reactionType: LikeDislikeReactionType): Observable<CommentReactionDTO>
  {
    return this.client.post<CommentReactionDTO>(`reactions/comment/${commentId}/react/${reactionType}`, {});
  }
  getReaction(commentId: string): Observable<CommentReactionDTO>
  {
    return this.client.get<CommentReactionDTO>(`reactions/comment/${commentId}/reaction`);
  }
}
