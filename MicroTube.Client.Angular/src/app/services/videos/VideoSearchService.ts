import { Injectable } from "@angular/core";
import { VideoSearchParametersDTO } from "../../data/DTO/VideoSearchDTO";
import { Router } from "@angular/router";

@Injectable({
  providedIn: "root"
})
export class VideoSearchService
{
  private readonly router: Router;
  private videoSearchParameters: VideoSearchParametersDTO | null = null;
  constructor(router: Router)
  {
    this.router = router;
  }
  setText(text: string)
  {
    if (this.videoSearchParameters == null)
      this.videoSearchParameters = { text: text, sort: null, timeFilter: null, lengthFilter: null };
    else
      this.videoSearchParameters.text = text;
  }
  setSort(sort: string)
  {
    if (this.videoSearchParameters)
      this.videoSearchParameters.sort = sort;
    else
      console.warn("Video search parameters were not initialized.");
  }
  setTimeFilter(filter: string)
  {
    if (this.videoSearchParameters)
      this.videoSearchParameters.timeFilter = filter;
    else
      console.warn("Video search parameters were not initialized.");
  }
  setLengthFilter(filter: string)
  {
    if (this.videoSearchParameters)
      this.videoSearchParameters.lengthFilter = filter;
    else
      console.warn("Video search parameters were not initialized.");
  }
  search()
  {
    console.log(this.videoSearchParameters);
    if (!this.videoSearchParameters)
    {
      console.warn("Video search parameters must be set before executing the search.");
      return;
    }
    console.log(this.videoSearchParameters);
    this.router.navigate(["/"], { queryParams: this.videoSearchParameters });
  }
  resetSearch()
  {
    console.log("reset");
    this.videoSearchParameters = null;
  }
}
