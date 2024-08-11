export interface VideoSearchParametersDTO
{
  text: string | null;
  sort: string | null;
  timeFilter: string | null;
  lengthFilter: string | null;
  batchSize: number;
}
export interface SearchControlsDTO
{
  lengthFilterOptions: string[];
  timeFilterOptions: string[];
  sortOptions: string[];
}
