import { VideoDTO, VideoRawDTO } from "./VideoDTO";

export class VideoSearchResultDTO
{
  videos: VideoDTO[];
  meta: string | null;
  constructor(raw: VideoSearchResultRawDTO)
  {
    this.videos = raw.videos.map(_ => new VideoDTO(_));
    this.meta = raw.meta;
  }
}
export interface VideoSearchResultRawDTO
{
  videos: VideoRawDTO[];
  meta: string | null;
}
