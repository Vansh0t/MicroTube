import { Component, OnDestroy, OnInit, ViewChild } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { VideoService } from "../../services/videos/VideoService";
import { BehaviorSubject, Subscription } from "rxjs";
import { VideoDTO } from "../../data/DTO/VideoDTO";
import mime from "mime";
import { NgxPlayerComponent, NgxPlayerOptions, QualityOption } from "../ngx-player/ngx-player.component";
import { TimeFormatter } from "../../services/formatting/TimeFormatter";
import { DateTime } from "luxon";
import { VgPlayerPlaytimeTracker } from "../../services/videos/VgPlayerPlaytimeTracker";
import { VgApiService } from "@videogular/ngx-videogular/core";
import { VideoSearchService } from "../../services/videos/VideoSearchService";

@Component({
  selector: "video-watch",
  templateUrl: "./video-watch.component.html",
  styleUrls: ["./video-watch.component.css"]
})
export class VideoWatchComponent implements OnInit, OnDestroy
{
  private readonly REPORT_VIEW_TIMEOUT_SECONDS = 30;
  private readonly route: ActivatedRoute;
  private readonly router: Router;
  private readonly videoService: VideoService;
  private readonly videoSearchService: VideoSearchService;
  private readonly timeFormatter: TimeFormatter;
  private playtimeTracker: VgPlayerPlaytimeTracker | null = null;
  private videoPlayerOptions: NgxPlayerOptions | null = null;
  private videoId: string | null = null;
  private userAuthStateSubscription: Subscription | null = null;
  private userLikeSubscription: Subscription | null = null;
  @ViewChild("player") player!: NgxPlayerComponent;
  video$: BehaviorSubject<VideoDTO | null> = new BehaviorSubject<VideoDTO | null>(null);
  constructor(
    route: ActivatedRoute,
    router: Router,
    videoService: VideoService,
    timeFormatter: TimeFormatter,
    videoSearchService: VideoSearchService)
  {
    this.route = route;
    this.router = router;
    this.videoService = videoService;
    this.timeFormatter = timeFormatter;
    this.videoSearchService = videoSearchService;
  }
  ngOnDestroy(): void
  {
    this.userAuthStateSubscription?.unsubscribe();
    this.userAuthStateSubscription = null;
    this.userLikeSubscription?.unsubscribe();
    this.userLikeSubscription = null;
    this.playtimeTracker?.dispose();
  }
  ngOnInit(): void
  {
    this.videoId = this.route.snapshot.paramMap.get("id");
    if (this.videoId == null)
    {
      this.router.navigate(["/"]);
      return;
    }
    this.videoService.getVideo(this.videoId).subscribe(this.video$);
  }
  onApi(api: VgApiService)
  {
    this.playtimeTracker = new VgPlayerPlaytimeTracker(api);
    api.getDefaultMedia().subscriptions.loadedMetadata.subscribe(() =>
    {
      if (!this.playtimeTracker)
        return;
      const viewReportTime = Math.min(this.REPORT_VIEW_TIMEOUT_SECONDS, api.duration - 1);
      this.playtimeTracker.onPlaytime(viewReportTime, () =>
      {
        this.videoService.reportView(this.videoId!).subscribe();
      }
      );
    });
    
  }
  getVideoPlayerOptions(videoUrls: string): NgxPlayerOptions
  {
    if (this.videoPlayerOptions)
      return this.videoPlayerOptions;
    const splitUrls = videoUrls.split(";");
    const qualityOptions: QualityOption[] = splitUrls.map(_ =>
    {
      const videoMimeType = mime.getType(_);
      const height = parseInt(this.getQualityTierFromUrl(_));
      return {
        label: height + "p",
        height: height,
        sourceUrl: _,
        mediaType: videoMimeType == null ? "" : videoMimeType
      };
    }).sort((a, b) => b.height - a.height);
    this.videoPlayerOptions = {
      qualityOptions: qualityOptions,
      selectedQualityIndex: qualityOptions.length - (qualityOptions.length-1)
    };
    return this.videoPlayerOptions;
  }
  getQualityTierFromUrl(url: string)
  {
    const filename = url.split("/").pop();
    if (!filename)
      throw new Error("Invalid url");
    const tierString = filename.split(".")[0].split("_").pop();
    return tierString ? tierString: "";
  }
  getUploadTimeText()
  {
    const video = this.video$.value;
    if (!video)
      return "";
    const uploadTimeLocal = video.uploadTime.toLocal();
    const nowLocal = DateTime.local();
    return this.timeFormatter.getUserFriendlyTimeDifference(uploadTimeLocal, nowLocal);
  }
  searchUploaderVideos()
  {
    const video = this.video$.value;
    if (!video)
    {
      return;
    }
    if (!video.uploaderId || !video.uploaderId.trim())
    {
      return;
    }
    this.videoSearchService.resetSearch();
    this.videoSearchService.setUploaderIdFilter(video.uploaderId.toLowerCase(), video.uploaderPublicUsername);
    this.videoSearchService.navigateWithQueryString();
  }
}
