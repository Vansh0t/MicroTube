import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { CommentDTO, CommentRawDTO } from "../../data/DTO/CommentDTO";
import { Observable, map } from "rxjs";
import { CommentRequestDTO } from "../../data/DTO/CommentRequestDTO";
import { EditCommentRequestDTO } from "../../data/DTO/EditCommentRequestDTO";
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
  comment(videoId: string, request: CommentRequestDTO): Observable<CommentDTO>
  {
    return this.client.post<CommentRawDTO>(`comments/video/${videoId}/comment`, request)
      .pipe(
        map(raw =>
        {
          return new CommentDTO(raw);
        }));
  }
  editComment(commentId: string, request: EditCommentRequestDTO): Observable<CommentDTO>
  {
    return this.client.post<CommentRawDTO>(`comments/video/${commentId}/edit`, request)
      .pipe(
        map(raw => {
        return new CommentDTO(raw);
      }));
  }
  deleteComment(commentId: string): Observable<CommentDTO>
  {
    return this.client.post<CommentRawDTO>(`comments/video/${commentId}/delete`, {})
      .pipe(
        map(raw =>
        {
          return new CommentDTO(raw);
        }));
  }
}
