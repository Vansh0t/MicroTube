import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from "@angular/core";
import { Subscription } from "rxjs";
import { SearchControlsDTO, VideoSearchParametersDTO } from "../../data/DTO/VideoSearchDTO";
import { FormControl } from "@angular/forms";
import { VideoSearchService } from "../../services/videos/VideoSearchService";
import { VideoSearchResultDTO } from "../../data/DTO/VideoSearchResultDTO";
import { getScrollTopPercent } from "../../services/utils";
import { VideoDTO } from "../../data/DTO/VideoDTO";

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
  readonly searchService: VideoSearchService;
  private readonly SCROLL_PERCENT_FOR_NEW_BATCH = 0.001;
  private searchSubscription: Subscription | null = null;
  private searchControlsFetchSubscription: Subscription | null = null;
  private videosSubscription: Subscription | null = null;
  private prevScrollPercent = 0;
  timeFilterControl = new FormControl();
  lengthFilterControl = new FormControl();
  sortControl = new FormControl();
  videos: VideoDTO[] | null = null;
  searchControls: SearchControlsDTO | null = null;
  get isLoadingVideos()
  {
    return this.videosSubscription != null;
  }
  constructor(searchService: VideoSearchService)
  {
    this.searchService = searchService;
  }
  ngOnDestroy()
  {
    this.searchSubscription?.unsubscribe();
    this.searchControlsFetchSubscription?.unsubscribe();
  }
  ngOnInit(): void
  {
    this.updatePaginationBatchSizeByScreenWidth(window.innerWidth);
    this.updateVideos(this.searchService.videoSearchParameters$.value);
    this.searchSubscription = this.searchService.videoSearchParameters$.subscribe(this.updateVideos.bind(this));
    this.searchControlsFetchSubscription = this.searchService.getSearchControls().subscribe(this.initControlsUI.bind(this));
  }
  updateVideos(params: VideoSearchParametersDTO| null)
  {
    this.videosSubscription?.unsubscribe();
    this.videos = null;
    this.getVideosBatch();
    if (!this.searchService.isSearch)
    {
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
  onScroll($event: Event)
  {
    if (!($event.target instanceof (HTMLElement)))
    {
      return;
    }
    const scrollTopPercent = getScrollTopPercent($event.target);
    if (this.isLoadingVideos)
    {
      this.prevScrollPercent = scrollTopPercent;
      return;

    }
    const scrollDelta = scrollTopPercent - this.prevScrollPercent;
    if (scrollDelta > 0 && scrollTopPercent  > 1 - this.SCROLL_PERCENT_FOR_NEW_BATCH)
    {
      this.updatePaginationBatchSizeByScreenWidth(window.innerWidth);
      this.getVideosBatch();
    }
    this.prevScrollPercent = scrollTopPercent;
  }
  private getVideosBatch()
  {
    this.videosSubscription = this.searchService.getVideos().subscribe(this.onNewVideosBatchReceived.bind(this));
  }
  private updatePaginationBatchSizeByScreenWidth(screenWidth: number)
  {
    const batchSize = this.BATCH_SIZES_FOR_SCREEN_WIDTH.findLast(_ => _.width < screenWidth)?.size;
    if (batchSize)
    {
      this.searchService.setBatchSize(batchSize);
      console.log("set pagination batch size to " + batchSize);
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
