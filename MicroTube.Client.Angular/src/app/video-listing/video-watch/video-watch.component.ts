import { Component, OnDestroy, OnInit, ViewChild } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { VideoService } from "../../services/videos/VideoService";
import { BehaviorSubject, Subscription } from "rxjs";
import { VideoDto } from "../../data/Dto/VideoDto";
import mime from "mime";
import { NgxPlayerComponent, NgxPlayerOptions, QualityOption } from "../ngx-player/ngx-player.component";
import { TimeFormatter } from "../../services/formatting/TimeFormatter";
import { DateTime } from "luxon";
import { VgPlayerPlaytimeTracker } from "../../services/videos/VgPlayerPlaytimeTracker";
import { VgApiService } from "@videogular/ngx-videogular/core";
import { QueryStringBuilder } from "../../services/query-string-processing/QueryStringBuilder";
import { MatDialog } from "@angular/material/dialog";
import { CommentPopupComponent } from "../../comments/comment-popup/comment-popup.component";
import { CommentsAreaComponent } from "../../comments/comments-area/comments-area.component";
import { AuthManager } from "../../services/auth/AuthManager";
import { AuthPopupComponent } from "../../auth/auth-popup/auth-popup.component";

@Component({
  selector: "video-watch",
  templateUrl: "./video-watch.component.html",
  styleUrls: ["./video-watch.component.scss"]
})
export class VideoWatchComponent implements OnInit, OnDestroy
{
  readonly VIDEO_COMMENT_TARGET_KEY = "video";
  private readonly REPORT_VIEW_TIMEOUT_SECONDS = 30;
  @ViewChild("player") player!: NgxPlayerComponent;
  video$: BehaviorSubject<VideoDto | null> = new BehaviorSubject<VideoDto | null>(null);
  @ViewChild("commentsArea") commentsArea!: CommentsAreaComponent;
  videoId: string | null = null;
  private readonly route: ActivatedRoute;
  private readonly router: Router;
  private readonly videoService: VideoService;
  private readonly timeFormatter: TimeFormatter;
  private readonly queryBuilder: QueryStringBuilder;
  private readonly dialog: MatDialog;
  private playtimeTracker: VgPlayerPlaytimeTracker | null = null;
  private videoPlayerOptions: NgxPlayerOptions | null = null;
  private userLikeSubscription: Subscription | null = null;
  readonly authManager: AuthManager;
  get endOfCommentsReached()
  {
    return this.commentsArea ? this.commentsArea.endOfDataReached : false;
  }
  get isLoadingComments()
  {
    return this.commentsArea ? this.commentsArea.isLoading : false;
  }
  constructor(
    route: ActivatedRoute,
    router: Router,
    videoService: VideoService,
    timeFormatter: TimeFormatter,
    queryBuilder: QueryStringBuilder,
    dialog: MatDialog,
    authManager: AuthManager)
  {
    this.dialog = dialog;
    this.route = route;
    this.router = router;
    this.videoService = videoService;
    this.timeFormatter = timeFormatter;
    this.queryBuilder = queryBuilder;
    this.authManager = authManager;
  }
  ngOnDestroy(): void
  {
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
      selectedQualityIndex: 0
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
    this.resetSearch();
    this.queryBuilder.setValue("uploaderIdFilter", video.uploaderId.toLowerCase());
    this.queryBuilder.setValue("uploaderAlias", video.uploaderPublicUsername);
    this.queryBuilder.navigate("/");
  }
  
  loadNextCommentsBatch()
  {
    if (this.commentsArea)
    {
      this.commentsArea.loadNextBatch();
    }
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
