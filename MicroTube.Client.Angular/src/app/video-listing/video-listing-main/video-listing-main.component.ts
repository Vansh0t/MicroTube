import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from "@angular/core";
import { Subscription } from "rxjs";
import { SearchControlsDTO, VideoSearchParametersDTO } from "../../data/DTO/VideoSearchDTO";
import { FormControl } from "@angular/forms";
import { VideoSearchService } from "../../services/videos/VideoSearchService";
import { VideoSearchResultDTO } from "../../data/DTO/VideoSearchResultDTO";
import { getScrollTopPercent } from "../../services/utils";
import { VideoDTO } from "../../data/DTO/VideoDTO";
import { VideoSearchQueryStringReader } from "../../services/query-string-processing/VideoSearchQueryStringReader";
import { QueryStringBuilder } from "../../services/query-string-processing/QueryStringBuilder";
import { NavigationEnd, Router } from "@angular/router";

@Component({
  selector: "video-listing-main",
  templateUrl: "./video-listing-main.component.html",
  styleUrls: ["./video-listing-main.component.scss"]
})
export class VideoListingMainComponent implements OnInit, OnDestroy
{
  @ViewChild("scrollContainer") scrollContainer!: ElementRef<HTMLElement>;
  readonly optionsFormats: { [id: string]: string; } = {
    Time: "Upload Time",
    LastDay: "This Day",
    LastWeek: "This Week",
    LastMonth: "This Month",
    LastSixMonths: "Last 6 Months",
    LastYear: "This Year",
  };
  private readonly BATCH_SIZES_FOR_SCREEN_WIDTH = [
    {
      width: 991,
      size: 18
    },
    {
      width: 1200,
      size: 20
    },
    

  ];
  private readonly searchQueryReader: VideoSearchQueryStringReader;
  private readonly queryBuilder: QueryStringBuilder;
  private readonly router: Router;
  readonly searchService: VideoSearchService;
  private searchSubscription: Subscription | null = null;
  private searchControlsFetchSubscription: Subscription | null = null;
  private videosSubscription: Subscription | null = null;
  private routerSubscription: Subscription | null = null;
  timeFilterControl = new FormControl();
  lengthFilterControl = new FormControl();
  sortControl = new FormControl();
  videos: VideoDTO[] | null = null;
  searchControls: SearchControlsDTO | null = null;
  endOfDataReached: boolean = false;
  get isLoadingVideos()
  {
    return this.videosSubscription != null;
  }
  constructor(searchService: VideoSearchService,
    searchQueryReader: VideoSearchQueryStringReader,
    queryBuilder: QueryStringBuilder,
    router: Router)
  {
    this.router = router;
    this.searchService = searchService;
    this.searchQueryReader = searchQueryReader;
    this.queryBuilder = queryBuilder;
  }
  ngOnDestroy()
  {
    this.searchSubscription?.unsubscribe();
    this.searchControlsFetchSubscription?.unsubscribe();
    this.routerSubscription?.unsubscribe();
  }
  ngOnInit(): void
  {
    this.updatePaginationBatchSizeByScreenWidth(window.innerWidth);
    this.routerSubscription = this.router.events.subscribe(value =>
    {
      if (value instanceof NavigationEnd)
      {
        this.updateVideos();
      }
    });
    this.updateVideos();
    this.searchControlsFetchSubscription = this.searchService.getSearchControls().subscribe(this.initControlsUI.bind(this));
  }
  updateVideos()
  {
    this.endOfDataReached = false;
    const searchParams = this.searchQueryReader.readSearchParameters();
    this.videos = null;
    this.searchService.resetMeta();
    this.getVideosBatch(searchParams);
    this.updateSearchControls(searchParams);
  }
  formatOption(key: string)
  {
    if (key in this.optionsFormats)
    {
      return this.optionsFormats[key];
    }
    return key;
  }
  cancelUploadSearch()
  {
    this.resetSearch();
    this.queryBuilder.navigate("/");
  }
  getCurrentSearchParameters()
  {
    const searchParams = this.searchQueryReader.readSearchParameters();
    return searchParams;
  }
  isSearchControlsShown()
  {
    const searchParams = this.searchQueryReader.readSearchParameters();
    return this.searchControls && (searchParams.text || searchParams.uploaderIdFilter);
  }
  getNextBatch()
  {
    this.updatePaginationBatchSizeByScreenWidth(window.innerWidth);
    const searchParams = this.searchQueryReader.readSearchParameters();
    this.getVideosBatch(searchParams);
  }
  private getVideosBatch(params: VideoSearchParametersDTO)
  {
    this.videosSubscription?.unsubscribe();
    this.videosSubscription = this.searchService.getVideos(params)
      .subscribe(this.onNewVideosBatchReceived.bind(this));
  }
  private updatePaginationBatchSizeByScreenWidth(screenWidth: number)
  {
    const batchSize = this.BATCH_SIZES_FOR_SCREEN_WIDTH.findLast(_ => _.width < screenWidth)?.size;
    if (batchSize)
    {
      this.queryBuilder.setValue("batchSize", batchSize); 
    }
  }
  private onNewVideosBatchReceived(result: VideoSearchResultDTO)
  {
    this.videosSubscription?.unsubscribe();
    this.videosSubscription = null;
    if (!this.videos)
    {
      this.videos = result.videos;
    }
    else
    {
      this.videos = this.videos.concat(result.videos);
    }
    if (result.videos.length == 0)
    {
      this.endOfDataReached = true;
    }
  }
  private updateSearchControls(params: VideoSearchParametersDTO)
  {
    if (params.text || params.uploaderIdFilter)
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
      this.queryBuilder.setValue("timeFilter", val);
      this.queryBuilder.navigate("/");
    });
    this.lengthFilterControl.valueChanges.subscribe((val) =>
    {
      this.queryBuilder.setValue("lengthFilter", val);
      this.queryBuilder.navigate("/");
    });
    this.sortControl.valueChanges.subscribe((val) =>
    {
      this.queryBuilder.setValue("sort", val);
      this.queryBuilder.navigate("/");
    });
  }
  private resetSearch() //TO DO: move this to an extension method
  {
    this.queryBuilder.setValue("text", null); 
    this.queryBuilder.setValue("sort", null); 
    this.queryBuilder.setValue("timeFilter", null); 
    this.queryBuilder.setValue("lengthFilter", null); 
    this.queryBuilder.setValue("uploaderIdFilter", null); 
    this.queryBuilder.setValue("uploaderAlias", null);
  }
}
