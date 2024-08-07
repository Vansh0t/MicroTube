import { HttpClient, HttpParams } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { VideoDTO, VideoRawDTO, VideoUploadDTO } from "../../data/DTO/VideoDTO";
import { Observable, map } from "rxjs";
import { VideoUploadProgressDTO } from "../../data/DTO/VideoUploadProgressDTO";
import { DateTime, Duration } from "luxon";
import { VideoSearchSuggestion } from "../../data/DTO/VideoSearchSuggestionDTO";
import { SearchControlsDTO, VideoSearchParametersDTO } from "../../data/DTO/VideoSearchDTO";


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

  getVideos() : Observable<VideoDTO[]>
  {
    const result = this.client.get<VideoRawDTO[]>("Videos")
      .pipe(
        map(response =>
        {
          return response.map(raw => new VideoDTO(raw));
        })
      );
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
            dto.timestampHuman = DateTime.fromISO(dto.timestamp).toLocal().toLocaleString(DateTime.DATETIME_SHORT);
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
  getSearchSuggestions(text: string): Observable<VideoSearchSuggestion[]>
  {
    if (text && !text.trim())
    {
      throw new Error("Empty text string provided.");
    }
    const result = this.client.get<VideoSearchSuggestion[]>("Videos/VideosSearch/suggestions/" + text);
    return result;
  }
  searchVideos(parameters: VideoSearchParametersDTO): Observable<VideoDTO[]>
  {
    if (!parameters.text.trim())
    {
      throw new Error("Empty text string provided.");
    }
    let urlParams = new HttpParams();
    urlParams = urlParams.set("text", parameters.text);
    if (parameters.sort)
      urlParams = urlParams.set("sort", parameters.sort);
    if (parameters.lengthFilter)
      urlParams = urlParams.set("lengthFilter", parameters.lengthFilter);
    if (parameters.timeFilter)
      urlParams = urlParams.set("timeFilter", parameters.timeFilter);
    return this.searchVideosByQueryString(urlParams.toString());
  }
  searchVideosByQueryString(parameters: string): Observable<VideoDTO[]>
  {
    if (!parameters.trim())
    {
      throw new Error("Empty parameters string provided.");
    }
    const result = this.client.get<VideoRawDTO[]>("Videos/VideosSearch/videos?" + parameters).pipe(
      map(response =>
      {
        return response.map(raw => new VideoDTO(raw));
      })
    );
    return result;
  }
  getSearchControls(): Observable<SearchControlsDTO>
  {
    const result = this.client.get<SearchControlsDTO>("Videos/VideosSearch/controls");
    return result;
  }
}
