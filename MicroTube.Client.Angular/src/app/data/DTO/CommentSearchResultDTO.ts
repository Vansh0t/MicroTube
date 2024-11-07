import { CommentDTO, CommentRawDTO } from "./CommentDTO";
export class CommentSearchResultDTO
{
  comments: CommentDTO[];
  meta: string | null;
  constructor(raw: CommentSearchResultRawDTO)
  {
    this.comments = raw.comments.map(_ => new CommentDTO(_));
    this.meta = raw.meta;
  }
}
export interface CommentSearchResultRawDTO
{
  comments: CommentRawDTO[];
  meta: string|null;
}
