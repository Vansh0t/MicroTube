import { Injectable } from "@angular/core";
import { SearchControlsDto, VideoSearchParametersDto } from "../../data/Dto/VideoSearchDto";
import { Observable, map } from "rxjs";
import { VideoSearchMetaDto } from "../../data/Dto/VideoSearchMetaDto";
import { HttpClient, HttpParams } from "@angular/common/http";
import { VideoSearchSuggestion } from "../../data/Dto/VideoSearchSuggestionDto";
import { VideoSearchResultDto, VideoSearchResultRawDto } from "../../data/Dto/VideoSearchResultDto";

@Injectable({
  providedIn: "root"
})
export class VideoSearchService
{
  private meta: VideoSearchMetaDto = { meta: null };
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
  getVideos(params: VideoSearchParametersDto): Observable<VideoSearchResultDto>
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
  getSearchControls(): Observable<SearchControlsDto>
  {
    const result = this.client.get<SearchControlsDto>("videos/videossearch/controls");
    return result;
  }
  resetMeta()
  {
    this.meta.meta = null;
  }
  private searchVideosByQueryString(parameters: string): Observable<VideoSearchResultDto>
  {
    if (!parameters.trim())
    {
      throw new Error("Empty parameters string provided.");
    }
    const result = this.client.post<VideoSearchResultRawDto>("videos/videossearch/videos?" + parameters, this.meta).pipe(
      map(response =>
      {
        this.meta.meta = response.meta;
        return new VideoSearchResultDto(response);
      })
    );
    return result;
  }
}
