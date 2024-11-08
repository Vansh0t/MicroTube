import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { CommentDto, CommentRawDto } from "../../data/Dto/CommentDto";
import { Observable, map } from "rxjs";
import { CommentRequestDto } from "../../data/Dto/CommentRequestDto";
import { EditCommentRequestDto } from "../../data/Dto/EditCommentRequestDto";


@Injectable({
  providedIn: "root"
})
export class CommentingService
{
  private readonly client: HttpClient;
  constructor(client: HttpClient)
  {
    this.client = client;
  }
  comment(targetKey: string, targetId: string, request: CommentRequestDto): Observable<CommentDto>
  {
    return this.client.post<CommentRawDto>(`comments/${targetKey}/${targetId}/comment`, request)
      .pipe(
        map(raw =>
        {
          return new CommentDto(raw);
        }));
  }
  editComment(targetKey: string, commentId: string, request: EditCommentRequestDto): Observable<CommentDto>
  {
    return this.client.post<CommentRawDto>(`comments/${targetKey}/${commentId}/edit`, request)
      .pipe(
        map(raw => {
        return new CommentDto(raw);
      }));
  }
  deleteComment(targetKey: string, commentId: string): Observable<CommentDto>
  {
    return this.client.post<CommentRawDto>(`comments/${targetKey}/${commentId}/delete`, {})
      .pipe(
        map(raw =>
        {
          return new CommentDto(raw);
        }));
  }
}
