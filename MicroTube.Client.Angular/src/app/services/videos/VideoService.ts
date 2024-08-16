import { HttpClient, HttpResponse } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { VideoDTO, VideoRawDTO, VideoUploadDTO } from "../../data/DTO/VideoDTO";
import { Observable, map } from "rxjs";
import { VideoUploadProgressDTO } from "../../data/DTO/VideoUploadProgressDTO";
import { DateTime, Duration } from "luxon";
import { VideoLikeDTO } from "../../data/DTO/VideoLikeDTO";
import { VideoDislikeDTO } from "../../data/DTO/VideoDislikeDTO";


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
  reportView(id: string): Observable<HttpResponse<null>>
  {
    const result = this.client.post<HttpResponse<null>>(`Videos/${id}/view`, {});
    return result;
  }
  dislikeVideo(id: string): Observable<VideoDislikeDTO>
  {
    const result = this.client.post<VideoDislikeDTO>(`Videos/${id}/dislike`, {});
    return result;
  }
  undislikeVideo(id: string): Observable<HttpResponse<null>>
  {
    const result = this.client.delete<HttpResponse<null>>(`Videos/${id}/dislike`);
    return result;
  }
  getVideoDislike(id: string): Observable<VideoDislikeDTO>
  {
    const result = this.client.get<VideoDislikeDTO>(`Videos/${id}/dislike`);
    return result;
  }
  likeVideo(id: string): Observable<VideoLikeDTO>
  {
    const result = this.client.post<VideoLikeDTO>(`Videos/${id}/like`, {});
    return result;
  }
  unlikeVideo(id: string): Observable<HttpResponse<null>>
  {
    const result = this.client.delete<HttpResponse<null>>(`Videos/${id}/like`);
    return result;
  }
  getVideoLike(id: string): Observable<VideoLikeDTO>
  {
    const result = this.client.get<VideoLikeDTO>(`Videos/${id}/like`);
    return result;
  }
  getVideo(id: string): Observable<VideoDTO>
  {
    const result = this.client.get<VideoRawDTO>("Videos/"+id)
      .pipe(
        map(response =>
        {
          return new VideoDTO(response);
        })
      );
    return result;
  }
  uploadVideo(data: VideoUploadDTO): Observable<VideoUploadProgressDTO>
  {
    const formData = new FormData();
    formData.append("title", data.title);
    if (data.description != null)
      formData.append("description", data.description);
    formData.append("file", data.file.files[0]);
    const result = this.client.post<VideoUploadProgressDTO>("Videos/VideoUpload", formData);
    return result;
  }
  getUploadProgressList(): Observable<VideoUploadProgressDTO[]>
  {
    const result = this.client.get<VideoUploadProgressDTO[]>("Videos/VideoUpload/Progress")
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
