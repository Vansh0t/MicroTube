import { Injectable } from "@angular/core";
import { VideoSearchParametersDTO } from "../../data/DTO/VideoSearchDTO";
import { ActivatedRoute, NavigationEnd, Router } from "@angular/router";
import { BehaviorSubject } from "rxjs";

@Injectable({
  providedIn: "root"
})
export class VideoSearchService
{
  videoSearchParameters$: BehaviorSubject<VideoSearchParametersDTO | null>;
  private readonly router: Router;
  private readonly activatedRoute: ActivatedRoute;
  private videoSearchParameters: VideoSearchParametersDTO | null = null;
  get isSearch()
  {
    return this.videoSearchParameters != null;
  }
  constructor(router: Router, activatedRoute: ActivatedRoute)
  {
    this.activatedRoute = activatedRoute;
    this.router = router;
    this.router.events.subscribe((event) =>
    {
      if (event instanceof (NavigationEnd))
        this.syncWithQueryString();
    });
    this.videoSearchParameters$ = new BehaviorSubject<VideoSearchParametersDTO | null>(this.videoSearchParameters);
  }
  setText(text: string)
  {
    if (text === this.videoSearchParameters?.text)
      return;
    if (this.videoSearchParameters == null)
      this.videoSearchParameters = { text: text, sort: null, timeFilter: null, lengthFilter: null };
    else
      this.videoSearchParameters.text = text;
    this.notifyParametersChanged();
  }
  setSort(sort: string)
  {
    if (this.videoSearchParameters?.sort === sort)
      return;
    if (this.videoSearchParameters)
    {
      this.videoSearchParameters.sort = sort;
      this.notifyParametersChanged();
    }
    else
      console.warn("Video search parameters were not initialized.");
    
  }
  setTimeFilter(filter: string)
  {
    if (this.videoSearchParameters?.timeFilter === filter)
      return;
    if (this.videoSearchParameters)
    {
      this.videoSearchParameters.timeFilter = filter;
      this.notifyParametersChanged();
    }
    else
      console.warn("Video search parameters were not initialized.");
  }
  setLengthFilter(filter: string)
  {
    if (this.videoSearchParameters?.lengthFilter === filter)
      return;
    if (this.videoSearchParameters)
    {
      this.videoSearchParameters.lengthFilter = filter;
      this.notifyParametersChanged();
    }
    else
      console.warn("Video search parameters were not initialized.");
  }
  navigateWithQueryString()
  {
    if (!this.videoSearchParameters)
    {
      this.router.navigate(["/"]);
      return;
    }
    this.router.navigate(["/"], { queryParams: this.videoSearchParameters });
  }
  resetSearch()
  {
    this.videoSearchParameters = null;
    this.notifyParametersChanged();
  }
  syncWithQueryString()
  {
    const params = this.parseQueryString();
    if (JSON.stringify(params) != JSON.stringify(this.videoSearchParameters))
    {
      this.videoSearchParameters = params;
      this.notifyParametersChanged();
    }
  }
  private parseQueryString(): VideoSearchParametersDTO | null
  {
    const textParam = <string>this.activatedRoute.snapshot.queryParams["text"]?.trim();
    if (!textParam)
      return null;
    const sortParam = <string>this.activatedRoute.snapshot.queryParams["sort"]?.trim();
    const timeFilterParam = <string>this.activatedRoute.snapshot.queryParams["timeFilter"]?.trim();
    const lengthFilterParam = <string>this.activatedRoute.snapshot.queryParams["lengthFilter"]?.trim();
    const params: VideoSearchParametersDTO = {
      text: textParam,
      sort: sortParam,
      timeFilter: timeFilterParam,
      lengthFilter: lengthFilterParam
    };
    return params;
  }
  private notifyParametersChanged()
  {
    this.videoSearchParameters$.next(this.videoSearchParameters);
  }
}
