import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { CommentDto, CommentRawDto } from "../../data/Dto/CommentDto";
import { Observable, map } from "rxjs";
import { CommentRequestDto } from "../../data/Dto/CommentRequestDto";
import { EditCommentRequestDto } from "../../data/Dto/EditCommentRequestDto";
import { ICommentingService } from "../ICommentingService";


@Injectable({
  providedIn: "root"
})
export class VideoCommentingService implements ICommentingService
{
  private readonly client: HttpClient;
  constructor(client: HttpClient)
  {
    this.client = client;
  }
  comment(videoId: string, request: CommentRequestDto): Observable<CommentDto>
  {
    return this.client.post<CommentRawDto>(`comments/video/${videoId}/comment`, request)
      .pipe(
        map(raw =>
        {
          return new CommentDto(raw);
        }));
  }
  editComment(commentId: string, request: EditCommentRequestDto): Observable<CommentDto>
  {
    return this.client.post<CommentRawDto>(`comments/video/${commentId}/edit`, request)
      .pipe(
        map(raw => {
        return new CommentDto(raw);
      }));
  }
  deleteComment(commentId: string): Observable<CommentDto>
  {
    return this.client.post<CommentRawDto>(`comments/video/${commentId}/delete`, {})
      .pipe(
        map(raw =>
        {
          return new CommentDto(raw);
        }));
  }
}
