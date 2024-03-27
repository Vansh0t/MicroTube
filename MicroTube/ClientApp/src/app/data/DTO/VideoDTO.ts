import { DateTime, Duration } from "luxon";
import { FileInput } from "ngx-custom-material-file-input";


/*public required string Id { get; set; }
		public required string Url { get; set; }
		public required string Title { get; set; }
		public required DateTime UploadTime { get; set; }
		public string ? Description { get; set; }
		public string ? ThumbnailUrls { get; set; }
		public string ? SnapshotUrls { get; set; }*/
export class VideoDTO
{
  id: string;
  url: string;
  title: string;
  uploadTime: DateTime;
  description: string | null;
  thumbnailUrls: string[] | undefined;
  snapshotUrls: string[] | undefined;
  length: Duration;
  lengthHuman: string | null;
  constructor(
    raw: VideoRawDTO
  )
  {
    this.id = raw.id;
    this.url = raw.url;
    this.title = raw.title;
    this.uploadTime = DateTime.fromISO(raw.uploadTime);
    this.description = raw.description;
    this.thumbnailUrls = raw.thumbnailUrls?.split(";"); 
    this.snapshotUrls = raw.snapshotUrls?.split(";");
    this.length = Duration.fromDurationLike({ seconds: raw.lengthSeconds });
    this.lengthHuman = this.length.toISOTime({ suppressMilliseconds: true });
  }
}

export interface VideoRawDTO
{
  id: string;
  url: string;
  title: string;
  uploadTime: string;
  description: string | null;
  thumbnailUrls: string | undefined;
  snapshotUrls: string | undefined;
  lengthSeconds: number;
}

export interface VideoUploadDTO
{
  title: string;
  description: string | null;
  file: FileInput;
}
