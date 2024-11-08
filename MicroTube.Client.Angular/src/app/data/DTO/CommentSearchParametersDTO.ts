import { CommentSortType } from "../../services/SortTypes";

export interface CommentSearchParametersDto
{
  sortType: CommentSortType;
  batchSize: number;
}
