import { Component, OnDestroy, OnInit } from "@angular/core";
import { VideoService } from "../../services/videos/VideoService";
import { Observable, Subscription } from "rxjs";
import { VideoDTO } from "../../data/DTO/VideoDTO";
import { ActivatedRoute, Router } from "@angular/router";
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
    Time: "Upload Time",
    LastDay: "This Day",
    LastWeek: "This Week",
    LastMonth: "This Month",
    LastSixMonths: "Last 6 Months",
    LastYear: "This Year",
  };
  private readonly videoService: VideoService;
  private readonly activatedRoute: ActivatedRoute;
  private readonly router: Router;
  readonly searchService: VideoSearchService;
  private searchSubscription: Subscription | null = null;
  private searchControlsFetchSubscription: Subscription | null = null;
  timeFilterControl = new FormControl();
  lengthFilterControl = new FormControl();
  sortControl = new FormControl();
  videos$: Observable<VideoDTO[]> | null = null;
  searchControls: SearchControlsDTO | null = null;
  constructor(videoService: VideoService, activatedRoute: ActivatedRoute, router: Router, searchService: VideoSearchService)
  {
    this.searchService = searchService;
    this.router = router;
    this.activatedRoute = activatedRoute;
    this.videoService = videoService;
  }
  ngOnDestroy()
  {
    this.searchSubscription?.unsubscribe();
    this.searchControlsFetchSubscription?.unsubscribe();
  }
  ngOnInit(): void
  {
    this.updateVideos(this.searchService.videoSearchParameters$.value);
    this.searchSubscription = this.searchService.videoSearchParameters$.subscribe(this.updateVideos.bind(this));
    this.searchControlsFetchSubscription = this.videoService.getSearchControls().subscribe(this.initControlsUI.bind(this));
  }
  updateVideos(params: VideoSearchParametersDTO| null)
  {
    if (!params)
    {
      this.videos$ = this.videoService.getVideos();
      return;
    }
    this.videos$ = this.videoService.searchVideos(params);
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
  
  private updateSearchControls(params: VideoSearchParametersDTO | null)
  {
    if (this.searchService.isSearch && params)
    {
      if (params.sort)
        this.sortControl.setValue(params.sort, { emitEvent: false });
      if (params.timeFilter)
        this.timeFilterControl.setValue(params.timeFilter, { emitEvent: false });
      if (params.lengthFilter)
        this.lengthFilterControl.setValue(params.lengthFilter, { emitEvent: false });
    }
  }
  private initControlsUI(controls: SearchControlsDTO)
  {
    this.searchControls = controls;
    this.timeFilterControl.valueChanges.subscribe((val) =>
    {
      this.searchService.setTimeFilter(val);
      this.searchService.navigateWithQueryString();
    });
    this.lengthFilterControl.valueChanges.subscribe((val) =>
    {
      this.searchService.setLengthFilter(val);
      this.searchService.navigateWithQueryString();
    });
    this.sortControl.valueChanges.subscribe((val) =>
    {
      this.searchService.setSort(val);
      this.searchService.navigateWithQueryString();
    });
  }
}
