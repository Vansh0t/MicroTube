import { CommentSortType } from "../../services/SortTypes";

export interface CommentSearchParametersDTO
{
  sortType: CommentSortType;
  batchSize: number;
}
