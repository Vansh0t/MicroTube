import { VideoDto, VideoRawDto } from "./VideoDto";

export class VideoSearchResultDto
{
  videos: VideoDto[];
  meta: string | null;
  constructor(raw: VideoSearchResultRawDto)
  {
    this.videos = raw.videos.map(_ => new VideoDto(_));
    this.meta = raw.meta;
  }
}
export interface VideoSearchResultRawDto
{
  videos: VideoRawDto[];
  meta: string | null;
}
