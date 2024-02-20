import { Component, OnInit } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { VideoService } from "../../services/videos/VideoService";
import { Observable } from "rxjs";
import { VideoDTO } from "../../data/DTO/VideoDTO";
import mime from "mime";

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
  buildVideoPlayerOptions(videoUrl :string)
  {
    const videoMimeType = mime.getType(videoUrl);
    return {
      fill: true,
      autoplay: false,
      controls: true,
      sources: [{
        src: videoUrl,
        type: videoMimeType == null ? "" : videoMimeType
      }],
      muted: false,
      responsive: true
    };
  }
}
