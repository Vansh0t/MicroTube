import { Observable } from "rxjs";
import { CommentDTO } from "../data/DTO/CommentDTO";
import { CommentRequestDTO } from "../data/DTO/CommentRequestDTO";
import { EditCommentRequestDTO } from "../data/DTO/EditCommentRequestDTO";

export interface ICommentingService
{
  comment(videoId: string, request: CommentRequestDTO): Observable<CommentDTO>;
  editComment(commentId: string, request: EditCommentRequestDTO): Observable<CommentDTO>;
  deleteComment(commentId: string): Observable<CommentDTO>;
}
