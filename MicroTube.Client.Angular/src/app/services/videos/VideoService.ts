/* eslint-disable @typescript-eslint/no-explicit-any */
import { HttpClient, HttpContext, HttpEvent, HttpEventType, HttpResponse } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { VideoDto, VideoRawDto, VideoUploadDto } from "../../data/Dto/VideoDto";
import { Observable, concatMap, filter, map, tap } from "rxjs";
import { VideoUploadProgressDto } from "../../data/Dto/VideoUploadProgressDto";
import { DateTime, Duration } from "luxon";
import { VideoUploadLinkDto } from "../../data/Dto/VideoUploadLinkDto";
import { VideoNotifyUploadDto } from "../../data/Dto/VideoNotifyUploadDto";
import mime from "mime";
import { IS_NO_API_REQUEST } from "../http/interceptors/InterceptorsShared";

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
  uploadVideo(data: VideoUploadDto, file: File, onFileUploadProgress: (event: HttpEvent<any>) => void): Observable<VideoUploadProgressDto>
  {
    let linkDto: VideoUploadLinkDto;
    const result = this.client.post<VideoUploadLinkDto>("videos/videoupload/link", data
    ).pipe(
      concatMap((linkResponse) =>
      {
        linkDto = linkResponse;
        return this.uploadVideoFileToRemoteStorage.bind(this, linkResponse, file, onFileUploadProgress)();
      }),
      filter((event): event is HttpResponse<any> => event.type == HttpEventType.Response),
      concatMap(() =>
      {
        return this.notifyVideoUploaded.bind(this, linkDto)();
      })
    );
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
  private uploadVideoFileToRemoteStorage(linkDto: VideoUploadLinkDto, file: File, onFileUploadProgress: (event: HttpEvent<any>) => void)
  {
    const context = new HttpContext();
    context.set(IS_NO_API_REQUEST, true);
    const result = this.client.put<HttpEvent<VideoUploadProgressDto>>(linkDto.link, file,
      {
        context: context,
        headers: {
          "x-ms-blob-type": "BlockBlob",
          "Content-Type": mime.getType(linkDto.generatedFileName)!,
          "x-ms-access-tier": "Cold"
        },
        reportProgress: true,
        observe: "events"
      }).pipe(
        tap(onFileUploadProgress)
      );
    return result;
  }
  private notifyVideoUploaded(linkDto: VideoUploadLinkDto)
  {
    const notifyDto: VideoNotifyUploadDto = {
      generatedFileName: linkDto.generatedFileName,
      generatedLocationName: linkDto.generatedRemoteLocationName
    };
    const result = this.client.post<VideoUploadProgressDto>("videos/videoupload/notify", notifyDto);
    return result;
  }
}
