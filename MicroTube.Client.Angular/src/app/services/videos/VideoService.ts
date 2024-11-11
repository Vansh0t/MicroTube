import { HttpClient, HttpEvent, HttpResponse } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { VideoDto, VideoRawDto, VideoUploadDto } from "../../data/Dto/VideoDto";
import { Observable, map } from "rxjs";
import { VideoUploadProgressDto } from "../../data/Dto/VideoUploadProgressDto";
import { DateTime, Duration } from "luxon";
import { UserVideoReactionDto } from "../../data/Dto/UserVideoReactionDto";
import { LikeDislikeReactionType } from "../ReactionTypes";


@Injectable({
  providedIn: "root"
})
export class VideoService
{
  private readonly client: HttpClient;

  constructor(client: HttpClient)
  {
    this.client = client;
  }
  reportView(videoId: string): Observable<HttpResponse<null>>
  {
    console.log("Reporting view");
    const result = this.client.post<HttpResponse<null>>(`videos/${videoId}/view`, {});
    return result;
  }
  getVideo(videoId: string): Observable<VideoDto>
  {
    const result = this.client.get<VideoRawDto>("videos/" + videoId)
      .pipe(
        map(response =>
        {
          return new VideoDto(response);
        })
      );
    return result;
  }
  uploadVideo(data: VideoUploadDto): Observable<HttpEvent<VideoUploadProgressDto>>
  {
    const formData = new FormData();
    formData.append("title", data.title);
    if (data.description != null)
      formData.append("description", data.description);
    formData.append("file", data.file.files[0]);
    const result = this.client.post<VideoUploadProgressDto>("videos/videoupload", formData,
      {
        reportProgress: true,
        observe: "events"
      });
    return result;
  }
  getUploadProgressList(): Observable<VideoUploadProgressDto[]>
  {
    const result = this.client.get<VideoUploadProgressDto[]>("videos/videoupload/Progress")
      .pipe(
        map(response =>
        {
          response.map(dto =>
          {
            dto.timestampHuman = DateTime.fromISO(dto.timestamp, { zone: "utc" }).toLocal().toLocaleString(DateTime.DATETIME_SHORT);
            if (dto.lengthSeconds != null)
            {
              const length = Duration.fromDurationLike({ seconds: dto.lengthSeconds });
              dto.lengthHuman = length.toISOTime({ suppressMilliseconds: true });

            }
            return dto;
          });
          return response;
        }
        ));
    return result;
  }
}
