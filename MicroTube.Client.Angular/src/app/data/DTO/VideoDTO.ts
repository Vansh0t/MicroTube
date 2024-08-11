import { DateTime, Duration } from "luxon";
import { FileInput } from "ngx-custom-material-file-input";
export class VideoDTO
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
  constructor(
    raw: VideoRawDTO
  )
  {
    this.id = raw.id;
    this.urls = raw.urls;
    this.title = raw.title;
    this.uploadTime = DateTime.fromISO(raw.uploadTime);
    this.description = raw.description;
    this.thumbnailUrls = raw.thumbnailUrls?.split(";"); 
    this.snapshotUrls = raw.snapshotUrls?.split(";");
    this.length = Duration.fromDurationLike({ seconds: raw.lengthSeconds });
    this.lengthHuman = this.length.toISOTime({ suppressMilliseconds: true });
    this.searchMeta = raw.searchMeta;
  }
}

export interface VideoRawDTO
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
}

export interface VideoUploadDTO
{
  title: string;
  description: string | null;
  file: FileInput;
}
