import { Component, Input, OnInit } from "@angular/core";
import { VideoDto } from "../../data/Dto/VideoDto";
import { Subscription, timer } from "rxjs";
import { TimeFormatter } from "../../services/formatting/TimeFormatter";
import { DateTime } from "luxon";
import { IntFormatter } from "../../services/formatting/IntFormatter";

@Component({
  selector: "video-card",
  templateUrl: "./video-card.component.html",
  styleUrls: ["./video-card.component.scss"]
})
export class VideoCardComponent implements OnInit
{
  private readonly thumbnailsRotationDelayMs = 1000;
  private readonly viewsFormatter: IntFormatter;
  private thumbnailsRotation: Subscription | null = null;
  private currentThumbnailIndex: number = -1;
  @Input() video: VideoDto | undefined = undefined;
  currentThumbnailSrc: string | undefined;
  timeFormatter: TimeFormatter;

  constructor(timeFormatter: TimeFormatter, viewsFormatter: IntFormatter)
  {
    this.timeFormatter = timeFormatter;
    this.viewsFormatter = viewsFormatter;
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
      this.currentThumbnailIndex = index;
    }
  }
  setDefaultThumbnail()
  {
    if (this.video != undefined && this.video.thumbnailUrls != null)
    {
      const midIndex = Math.floor(this.video.thumbnailUrls.length / 2);
      console.log(midIndex);
      console.log(this.video.thumbnailUrls);
      this.setThumbnail(midIndex);
    }
  }
  getUploadTimeText()
  {
    if (this.video == null)
      return "";
    const uploadTimeLocal = this.video.uploadTime.toLocal();
    const nowLocal = DateTime.local();
    return this.timeFormatter.getUserFriendlyTimeDifference(uploadTimeLocal, nowLocal);
  }
  getViewsText()
  {
    if (this.video == null)
      return "";
    return this.viewsFormatter.getUserFriendlyInt(this.video.views);
  }
}
