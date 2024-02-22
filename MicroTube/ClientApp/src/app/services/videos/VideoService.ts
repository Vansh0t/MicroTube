import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { VideoDTO, VideoRawDTO, VideoUploadDTO } from "../../data/DTO/VideoDTO";
import { Observable, map } from "rxjs";
import { VideoUploadProgressDTO } from "../../data/DTO/VideoUploadProgressDTO";


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
}
