import { Injectable } from "@angular/core";
import { SearchControlsDTO, VideoSearchParametersDTO } from "../../data/DTO/VideoSearchDTO";
import { Observable, map } from "rxjs";
import { VideoSearchMetaDTO } from "../../data/DTO/VideoSearchMetaDTO";
import { HttpClient, HttpParams } from "@angular/common/http";
import { VideoSearchSuggestion } from "../../data/DTO/VideoSearchSuggestionDTO";
import { VideoSearchResultDTO, VideoSearchResultRawDTO } from "../../data/DTO/VideoSearchResultDTO";

@Injectable({
  providedIn: "root"
})
export class VideoSearchService
{
  private meta: VideoSearchMetaDTO = { meta: null };
  private readonly client: HttpClient;
  private readonly DEFAULT_BATCH_SIZE = 20;
  constructor(client: HttpClient)
  {
    this.client = client;
  }
  getSearchSuggestions(text: string): Observable<VideoSearchSuggestion[]>
  {
    if (text && !text.trim())
    {
      throw new Error("Empty text string provided.");
    }
    const result = this.client.get<VideoSearchSuggestion[]>("videos/videossearch/suggestions/" + text);
    return result;
  }
  getVideos(params: VideoSearchParametersDTO): Observable<VideoSearchResultDTO>
  {
    let urlParams = new HttpParams();
    if (params.text)
    {
      urlParams = urlParams.set("text", params.text);
    }
    if (params.sort)
    {
      urlParams = urlParams.set("sort", params.sort);
    }
    if (params.lengthFilter)
    {
      urlParams = urlParams.set("lengthFilter", params.lengthFilter);
    }
    if (params.timeFilter)
    {
      urlParams = urlParams.set("timeFilter", params.timeFilter);
    }
    if (params.uploaderIdFilter)
    {
      urlParams = urlParams.set("uploaderIdFilter", params.uploaderIdFilter);
    }
    urlParams = urlParams.set("batchSize", params.batchSize ? params.batchSize.toString() : this.DEFAULT_BATCH_SIZE);
    return this.searchVideosByQueryString(urlParams.toString());
  }
  getSearchControls(): Observable<SearchControlsDTO>
  {
    const result = this.client.get<SearchControlsDTO>("videos/videossearch/controls");
    return result;
  }
  resetMeta()
  {
    this.meta.meta = null;
  }
  private searchVideosByQueryString(parameters: string): Observable<VideoSearchResultDTO>
  {
    if (!parameters.trim())
    {
      throw new Error("Empty parameters string provided.");
    }
    const result = this.client.post<VideoSearchResultRawDTO>("videos/videossearch/videos?" + parameters, this.meta).pipe(
      map(response =>
      {
        this.meta.meta = response.meta;
        return new VideoSearchResultDTO(response);
      })
    );
    return result;
  }
}
