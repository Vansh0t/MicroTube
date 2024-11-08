import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { LikeDislikeReactionType } from "../ReactionTypes";
import { LikeDislikeReactionDto } from "../../data/Dto/LikeDislikeReactionDto";


@Injectable({
  providedIn: "root"
})
export class LikeDislikeReactionService
{
  private readonly client: HttpClient;
  constructor(client: HttpClient)
  {
    this.client = client;
  }
  setReaction(targetKey: string, commentId: string, reactionType: LikeDislikeReactionType): Observable<LikeDislikeReactionDto>
  {
    return this.client.post<LikeDislikeReactionDto>(`reactions/${targetKey}/${commentId}/react/${reactionType}`, {});
  }
  getReaction(targetKey: string, commentId: string): Observable<LikeDislikeReactionDto>
  {
    return this.client.get<LikeDislikeReactionDto>(`reactions/${targetKey}/${commentId}/reaction`);
  }
}
