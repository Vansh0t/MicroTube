import { Component, OnInit } from "@angular/core";
import { VideoService } from "../../services/videos/VideoService";
import { Observable } from "rxjs";
import { VideoDTO } from "../../data/DTO/VideoDTO";

@Component({
  selector: "video-listing-main",
  templateUrl: "./video-listing-main.component.html",
  styleUrls: ["./video-listing-main.component.scss"]
})
export class VideoListingMainComponent implements OnInit
{
  private readonly videoService: VideoService;

  videos$: Observable<VideoDTO[]>| null = null;

  constructor(videoService: VideoService)
  {
    this.videoService = videoService;
  }

  ngOnInit(): void
  {
    this.videos$ = this.videoService.getVideos();
  }

}
