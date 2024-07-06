import { Component, OnDestroy, OnInit } from "@angular/core";
import { VideoService } from "../../services/videos/VideoService";
import { Observable, Subscription } from "rxjs";
import { VideoDTO } from "../../data/DTO/VideoDTO";
import { ActivatedRoute, Router } from "@angular/router";

@Component({
  selector: "video-listing-main",
  templateUrl: "./video-listing-main.component.html",
  styleUrls: ["./video-listing-main.component.scss"]
})
export class VideoListingMainComponent implements OnInit, OnDestroy
{
  private readonly videoService: VideoService;
  private readonly activatedRoute: ActivatedRoute;
  private readonly router: Router;
  private routerSubscription: Subscription | null = null;
  private prevSearchText: string | null = null;
  videos$: Observable<VideoDTO[]>| null = null;

  constructor(videoService: VideoService, activatedRoute: ActivatedRoute, router: Router)
  {
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
    this.routerSubscription = this.router.events.subscribe(() =>
    {
      this.updateVideos();
    });
  }
  updateVideos()
  {
    const videoSearchParam = <string>this.activatedRoute.snapshot.queryParams["videoSearch"]?.trim();

    if (videoSearchParam && this.prevSearchText != videoSearchParam)
    {
      this.prevSearchText = videoSearchParam;
      this.videos$ = this.videoService.searchVideos(videoSearchParam);
    }
    else if (!videoSearchParam)
    {
      this.prevSearchText = null;
      this.videos$ = this.videoService.getVideos();
    }
  }
}
