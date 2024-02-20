import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { VideoDTO, VideoRawDTO } from "../../data/DTO/VideoDTO";
import { Observable, map } from "rxjs";


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
}
