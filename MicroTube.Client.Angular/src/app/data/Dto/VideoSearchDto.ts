export interface VideoSearchParametersDto
{
  text: string | null;
  sort: string | null;
  timeFilter: string | null;
  lengthFilter: string | null;
  uploaderIdFilter: string | null;
  uploaderAlias: string | null;
  batchSize: number | null;
}

export interface SearchControlsDto
{
  lengthFilterOptions: string[];
  timeFilterOptions: string[];
  sortOptions: string[];
}
