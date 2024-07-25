export interface VideoSearchParametersDTO
{
    text: string;
    sort: string | null;
    timeFilter: string | null;
    lengthFilter: string | null;
}
export interface SearchControlsDTO
{
  lengthFilterOptions: string[];
  timeFilterOptions: string[];
  sortOptions: string[];
}
