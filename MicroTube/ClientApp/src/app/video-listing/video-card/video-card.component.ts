import { Component, Input, OnInit } from "@angular/core";
import { VideoDTO } from "../../data/DTO/VideoDTO";
import { Subscription, timer } from "rxjs";
import { TimeFormatter } from "../../services/formatting/TimeFormatter";
import { DateTime } from "luxon";

@Component({
  selector: "video-card",
  templateUrl: "./video-card.component.html",
  styleUrls: ["./video-card.component.css"]
})
export class VideoCardComponent implements OnInit
{
  private readonly thumbnailsRotationDelayMs = 1000;
  private thumbnailsRotation: Subscription | null = null;
  private currentThumbnailIndex: number = -1;
  @Input() video: VideoDTO | undefined = undefined;
  currentThumbnailSrc: string | undefined;
  timeFormatter: TimeFormatter;

  constructor(timeFormatter: TimeFormatter)
  {
    this.timeFormatter = timeFormatter;
  }

  ngOnInit(): void
  {
    this.setDefaultThumbnail();
  }
  getWatchRouterLink()
  {
    return "watch/" + this.video?.id;
  }
  onVideoPreviewHoverStart()
  {
    console.log("hover");
    this.thumbnailsRotation = timer(this.thumbnailsRotationDelayMs, this.thumbnailsRotationDelayMs)
      .subscribe({ next: this.nextThumbnail.bind(this) });
  }
  onVideoPreviewHoverEnd()
  {
    this.setDefaultThumbnail();
    this.thumbnailsRotation?.unsubscribe();
    this.thumbnailsRotation = null;
  }
  nextThumbnail()
  {
    if (this.video != undefined && this.video.thumbnailUrls != null)
    {
      if (this.currentThumbnailIndex == this.video.thumbnailUrls.length - 1)
        this.setThumbnail(0);
      else
        this.setThumbnail(this.currentThumbnailIndex+1);
    }
  }
  setThumbnail(index: number)
  {
    if (this.video != undefined && this.video.thumbnailUrls != null)
    {
      this.currentThumbnailSrc = this.video.thumbnailUrls[index];
      console.log(this.currentThumbnailSrc);
      this.currentThumbnailIndex = index;
    }
  }
  setDefaultThumbnail()
  {
    if (this.video != undefined && this.video.thumbnailUrls != null)
    {
      const midIndex = Math.floor(this.video.thumbnailUrls.length / 2);
      this.setThumbnail(midIndex);
    }
  }
  getUploadTimeText()
  {
    if (this.video == null)
      return "";
    const now = DateTime.utc();
    return this.timeFormatter.getUserFriendlyTimeDifference(this.video.uploadTime, now);
  }
}
