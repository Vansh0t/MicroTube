export interface VideoSearchParametersDTO
{
  text: string | null;
  sort: string | null;
  timeFilter: string | null;
  lengthFilter: string | null;
  uploaderIdFilter: string | null;
  uploaderAlias: string | null;
  batchSize: number | null;
}

export interface SearchControlsDTO
{
  lengthFilterOptions: string[];
  timeFilterOptions: string[];
  sortOptions: string[];
}
