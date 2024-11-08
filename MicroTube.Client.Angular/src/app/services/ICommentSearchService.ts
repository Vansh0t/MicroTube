import { Observable } from "rxjs";
import { CommentSearchParametersDto } from "../data/Dto/CommentSearchParametersDto";
import { CommentSearchResultDto } from "../data/Dto/CommentSearchResultDto";

export interface ICommentSearchService
{
  getComments(targetId: string, params: CommentSearchParametersDto): Observable<CommentSearchResultDto>;
  resetMeta():void;
}
