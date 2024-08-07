import { Component, OnInit } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { VideoService } from "../../services/videos/VideoService";
import { Observable } from "rxjs";
import { VideoDTO } from "../../data/DTO/VideoDTO";
import mime from "mime";
import { NgxPlayerOptions, QualityOption } from "../ngx-player/ngx-player.component";

@Component({
  selector: "video-watch",
  templateUrl: "./video-watch.component.html",
  styleUrls: ["./video-watch.component.css"]
})
export class VideoWatchComponent implements OnInit
{
  private readonly route: ActivatedRoute;
  private readonly router: Router;
  private readonly videoService: VideoService;
  private videoPlayerOptions: NgxPlayerOptions | null = null;
  private videoId: string | null = null;

  video$: Observable<VideoDTO> | null = null;
  constructor(route: ActivatedRoute, router: Router, videoService: VideoService)
  {
    this.route = route;
    this.router = router;
    this.videoService = videoService;
  }
  ngOnInit(): void
  {
    
    this.videoId = this.route.snapshot.paramMap.get("id");
    if (this.videoId == null)
    {
      this.router.navigate(["/"]);
      return;
    }
    this.video$ = this.videoService.getVideo(this.videoId);
    
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
}
