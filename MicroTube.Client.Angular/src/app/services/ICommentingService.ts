import { Observable } from "rxjs";
import { CommentDto } from "../data/Dto/CommentDto";
import { CommentRequestDto } from "../data/Dto/CommentRequestDto";
import { EditCommentRequestDto } from "../data/Dto/EditCommentRequestDto";

export interface ICommentingService
{
  comment(videoId: string, request: CommentRequestDto): Observable<CommentDto>;
  editComment(commentId: string, request: EditCommentRequestDto): Observable<CommentDto>;
  deleteComment(commentId: string): Observable<CommentDto>;
}
