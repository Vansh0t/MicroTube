import { DateTime, Duration } from "luxon";
import { FileInput } from "ngx-custom-material-file-input";
import { LikeDislikeReactionsAggregationDto } from "./LikeDislikeReactionsAggregationDto";
export class VideoDto
{
  id: string;
  urls: string;
  title: string;
  uploadTime: DateTime;
  description: string | null;
  thumbnailUrls: string[] | undefined;
  snapshotUrls: string[] | undefined;
  length: Duration;
  lengthHuman: string | null;
  searchMeta: string | null;
  views: number;
  uploaderPublicUsername: string | null;
  uploaderId: string | null;
  commentsCount: number;
  reactionsAggregation: LikeDislikeReactionsAggregationDto;
  constructor(
    raw: VideoRawDto
  )
  {
    this.id = raw.id;
    this.urls = raw.urls;
    this.title = raw.title;
    this.uploadTime = DateTime.fromISO(raw.uploadTime, { zone: "utc" });
    this.description = raw.description;
    this.thumbnailUrls = raw.thumbnailUrls?.split(";"); 
    this.snapshotUrls = raw.snapshotUrls?.split(";");
    this.length = Duration.fromDurationLike({ seconds: raw.lengthSeconds });
    this.lengthHuman = this.length.toISOTime({ suppressMilliseconds: true });
    this.searchMeta = raw.searchMeta;
    this.reactionsAggregation = raw.reactionsAggregation;
    this.views = raw.views;
    this.uploaderPublicUsername = raw.uploaderPublicUsername;
    this.uploaderId = raw.uploaderId;
    this.commentsCount = raw.commentsCount;
  }
}

export interface VideoRawDto
{
  id: string;
  urls: string;
  title: string;
  uploadTime: string;
  description: string | null;
  thumbnailUrls: string | undefined;
  snapshotUrls: string | undefined;
  lengthSeconds: number;
  searchMeta: string | null;
  views: number;
  uploaderPublicUsername: string | null;
  uploaderId: string | null;
  commentsCount: number;
  reactionsAggregation: LikeDislikeReactionsAggregationDto;

}

export interface VideoUploadDto
{
  title: string;
  description: string | null;
  file: FileInput;
}
