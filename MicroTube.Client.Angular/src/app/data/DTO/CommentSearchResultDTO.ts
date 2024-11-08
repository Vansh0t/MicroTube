import { CommentDto, CommentRawDto } from "./CommentDto";
export class CommentSearchResultDto
{
  comments: CommentDto[];
  meta: string | null;
  constructor(raw: CommentSearchResultRawDto)
  {
    this.comments = raw.comments.map(_ => new CommentDto(_));
    this.meta = raw.meta;
  }
}
export interface CommentSearchResultRawDto
{
  comments: CommentRawDto[];
  meta: string|null;
}
