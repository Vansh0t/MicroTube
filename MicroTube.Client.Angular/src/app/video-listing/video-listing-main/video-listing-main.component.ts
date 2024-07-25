import { Component, OnDestroy, OnInit } from "@angular/core";
import { VideoService } from "../../services/videos/VideoService";
import { Observable, Subscription } from "rxjs";
import { VideoDTO } from "../../data/DTO/VideoDTO";
import { ActivatedRoute, NavigationEnd, Router } from "@angular/router";
import { SearchControlsDTO, VideoSearchParametersDTO } from "../../data/DTO/VideoSearchDTO";
import { FormControl } from "@angular/forms";
import { VideoSearchService } from "../../services/videos/VideoSearchService";

@Component({
  selector: "video-listing-main",
  templateUrl: "./video-listing-main.component.html",
  styleUrls: ["./video-listing-main.component.scss"]
})
export class VideoListingMainComponent implements OnInit, OnDestroy
{
  readonly optionsFormats: { [id: string]: string; } = {
    LastDay: "This Day",
    LastWeek: "This Week",
    LastMonth: "This Month",
    LastSixMonths: "Last 6 Months",
    LastYear: "This Year",
  };
  private readonly videoService: VideoService;
  private readonly activatedRoute: ActivatedRoute;
  private readonly router: Router;
  private readonly searchService: VideoSearchService;
  private routerSubscription: Subscription | null = null;
  private prevSearchParameters: VideoSearchParametersDTO | null = null;
  timeFilterControl = new FormControl();
  lengthFilterControl = new FormControl();
  sortControl = new FormControl();
  videos$: Observable<VideoDTO[]> | null = null;
  searchControls$: Observable<SearchControlsDTO> | null = null;
  get isSearch(): boolean
  {
    return this.prevSearchParameters != null;
  }
  constructor(videoService: VideoService, activatedRoute: ActivatedRoute, router: Router, searchService: VideoSearchService)
  {
    this.searchService = searchService;
    this.router = router;
    this.activatedRoute = activatedRoute;
    this.videoService = videoService;
  }
  ngOnDestroy()
  {
    this.routerSubscription?.unsubscribe();
  }
  ngOnInit(): void
  {
    this.updateVideos();
    this.routerSubscription = this.router.events.subscribe((event) =>
    {
      if (event instanceof (NavigationEnd))
        this.updateVideos();
    });
    this.initControlsUI();
  }
  updateVideos()
  {
    const params = this.parseQueryString();
    if (params && JSON.stringify(this.prevSearchParameters) != JSON.stringify(params))
    {
      this.prevSearchParameters = params;
      this.videos$ = this.videoService.searchVideos(params);
    }
    else if (!params)
    {
      this.prevSearchParameters = null;
      this.searchService.resetSearch();
      this.videos$ = this.videoService.getVideos();
      return;
    }
    this.updateSearchControls(params);
  }
  formatOption(key: string)
  {
    if (key in this.optionsFormats)
    {
      return this.optionsFormats[key];
    }
    return key;
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
  private updateSearchControls(params: VideoSearchParametersDTO | null)
  {
    if (this.isSearch && params)
    {
      this.searchControls$ = this.videoService.getSearchControls();
      if (params.sort)
        this.sortControl.setValue(params.sort, { emitEvent: false });
      if (params.timeFilter)
        this.timeFilterControl.setValue(params.timeFilter, { emitEvent: false });
      if (params.lengthFilter)
        this.lengthFilterControl.setValue(params.lengthFilter, { emitEvent: false });
    }
  }
  private initControlsUI()
  {
    this.timeFilterControl.valueChanges.subscribe((val) =>
    {
      if (val)
      {
        this.searchService.setTimeFilter(val);
        this.searchService.search();
      }
    });
    this.lengthFilterControl.valueChanges.subscribe((val) =>
    {
      if (val)
      {
        this.searchService.setLengthFilter(val);
        this.searchService.search();
      }
    });
    this.sortControl.valueChanges.subscribe((val) =>
    {
      if (val)
      {
        this.searchService.setSort(val);
        this.searchService.search();
      }
    });
  }
}
