import { HttpClient, HttpEvent, HttpResponse } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { VideoDTO, VideoRawDTO, VideoUploadDTO } from "../../data/DTO/VideoDTO";
import { Observable, map } from "rxjs";
import { VideoUploadProgressDTO } from "../../data/DTO/VideoUploadProgressDTO";
import { DateTime, Duration } from "luxon";
import { UserVideoReactionDTO } from "../../data/DTO/UserVideoReactionDTO";
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
  react(videoId: string, reaction: LikeDislikeReactionType): Observable<UserVideoReactionDTO>
  {
    const result = this.client.post<UserVideoReactionDTO>(`videos/${videoId}/reaction/${reaction.toString()}`, {});
    return result;
  }
  getReaction(videoId: string): Observable<UserVideoReactionDTO>
  {
    const result = this.client.get<UserVideoReactionDTO>(`videos/${videoId}/reaction`);
    return result;
  }
  getVideo(videoId: string): Observable<VideoDTO>
  {
    const result = this.client.get<VideoRawDTO>("videos/" + videoId)
      .pipe(
        map(response =>
        {
          return new VideoDTO(response);
        })
      );
    return result;
  }
  uploadVideo(data: VideoUploadDTO): Observable<HttpEvent<VideoUploadProgressDTO>>
  {
    const formData = new FormData();
    formData.append("title", data.title);
    if (data.description != null)
      formData.append("description", data.description);
    formData.append("file", data.file.files[0]);
    const result = this.client.post<VideoUploadProgressDTO>("videos/videoupload", formData,
      {
        reportProgress: true,
        observe: "events"
      });
    return result;
  }
  getUploadProgressList(): Observable<VideoUploadProgressDTO[]>
  {
    const result = this.client.get<VideoUploadProgressDTO[]>("videos/videoupload/Progress")
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
