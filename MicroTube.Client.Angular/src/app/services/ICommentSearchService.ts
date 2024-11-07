import { Observable } from "rxjs";
import { CommentSearchParametersDTO } from "../data/DTO/CommentSearchParametersDTO";
import { CommentSearchResultDTO } from "../data/DTO/CommentSearchResultDTO";

export interface ICommentSearchService
{
  getComments(targetId: string, params: CommentSearchParametersDTO): Observable<CommentSearchResultDTO>;
  resetMeta():void;
}
