import { Injectable } from "@angular/core";
import { SearchControlsDTO, VideoSearchParametersDTO } from "../../data/DTO/VideoSearchDTO";
import { ActivatedRoute, NavigationEnd, Router } from "@angular/router";
import { BehaviorSubject, Observable, map } from "rxjs";
import { VideoSearchMetaDTO } from "../../data/DTO/VideoSearchMetaDTO";
import { HttpClient, HttpParams } from "@angular/common/http";
import { VideoSearchSuggestion } from "../../data/DTO/VideoSearchSuggestionDTO";
import { VideoSearchResultDTO, VideoSearchResultRawDTO } from "../../data/DTO/VideoSearchResultDTO";

@Injectable({
  providedIn: "root"
})
export class VideoSearchService
{
  videoSearchParameters$: BehaviorSubject<VideoSearchParametersDTO | null>;
  private readonly router: Router;
  private readonly activatedRoute: ActivatedRoute;
  private videoSearchParameters: VideoSearchParametersDTO | null = null;
  private meta: VideoSearchMetaDTO = { meta: null };
  private readonly client: HttpClient;
  private readonly DEFAULT_BATCH_SIZE = 20;
  get isSearch()
  {
    return this.videoSearchParameters != null
      && (this.videoSearchParameters.text != null || this.videoSearchParameters.uploaderIdFilter != null);
  }
  constructor(router: Router, activatedRoute: ActivatedRoute, client: HttpClient)
  {
    this.client = client;
    this.activatedRoute = activatedRoute;
    this.router = router;
    this.router.events.subscribe((event) =>
    {
      
      if (event instanceof (NavigationEnd))
      {
        console.log("TT");
        this.syncWithQueryString();
        this.meta.meta = null; 
      }
    });
    this.videoSearchParameters$ = new BehaviorSubject<VideoSearchParametersDTO | null>(this.videoSearchParameters);
    this.videoSearchParameters$.subscribe(() => { this.meta.meta = null; });
  }
  setText(text: string)
  {
    if (text === this.videoSearchParameters?.text)
      return;
    if (this.videoSearchParameters == null)
      this.videoSearchParameters = {
        text: text,
        sort: null,
        timeFilter: null,
        lengthFilter: null,
        batchSize: this.DEFAULT_BATCH_SIZE,
        uploaderIdFilter: null,
        uploaderAlias: null
      };
    else
      this.videoSearchParameters.text = text;
    this.notifyParametersChanged();
  }
  setUploaderIdFilter(uploaderId: string|null, uploaderAlias: string | null)
  {
    if (this.videoSearchParameters?.uploaderIdFilter === uploaderId)
      return;
    if (this.videoSearchParameters == null)
      this.videoSearchParameters =
      {
        text: null,
        sort: null,
        timeFilter: null,
        lengthFilter: null,
        batchSize: this.DEFAULT_BATCH_SIZE,
        uploaderIdFilter: uploaderId,
        uploaderAlias: uploaderAlias
      };
    else
      this.videoSearchParameters.uploaderIdFilter = uploaderId;
    this.notifyParametersChanged();
  }
  setSort(sort: string)
  {
    if (this.videoSearchParameters?.sort === sort)
      return;
    if (this.videoSearchParameters == null)
      this.videoSearchParameters =
      {
        text: null,
        sort: sort,
        timeFilter: null,
        lengthFilter: null,
        batchSize: this.DEFAULT_BATCH_SIZE,
        uploaderIdFilter: null,
        uploaderAlias: null
      };
    else 
      this.videoSearchParameters.sort = sort;
    this.notifyParametersChanged();
    
  }
  setBatchSize(batchSize: number)
  {
    if (this.videoSearchParameters?.batchSize === batchSize)
      return;
    if (this.videoSearchParameters == null)
      this.videoSearchParameters =
      {
        text: null,
        sort: null,
        timeFilter: null,
        lengthFilter: null,
        batchSize: batchSize,
        uploaderIdFilter: null,
        uploaderAlias: null
      };
    else
      this.videoSearchParameters.batchSize = batchSize;
    this.notifyParametersChanged();
  }
  setTimeFilter(filter: string)
  {
    if (this.videoSearchParameters?.timeFilter === filter)
      return;
    if (this.videoSearchParameters == null)
      this.videoSearchParameters =
      {
        text: null,
        sort: null,
        timeFilter: filter,
        lengthFilter: null,
        batchSize: this.DEFAULT_BATCH_SIZE,
        uploaderIdFilter: null,
        uploaderAlias: null
      };
    else
      this.videoSearchParameters.timeFilter = filter;
    this.notifyParametersChanged();
  }
  setLengthFilter(filter: string)
  {
    if (this.videoSearchParameters?.lengthFilter === filter)
      return;
    if (this.videoSearchParameters == null)
      this.videoSearchParameters =
      {
        text: null,
        sort: null,
        timeFilter: null,
        lengthFilter: filter,
        batchSize: this.DEFAULT_BATCH_SIZE,
        uploaderIdFilter: null,
        uploaderAlias: null
      };
    else
      this.videoSearchParameters.lengthFilter = filter;
    this.notifyParametersChanged();
  }
  navigateWithQueryString()
  {
    if (!this.videoSearchParameters)
    {
      this.router.navigate(["/"]);
      return;
    }
    this.router.navigate(["/"], { queryParams: this.videoSearchParameters });
  }
  resetSearch()
  {
    this.videoSearchParameters = null;
    this.notifyParametersChanged();
  }
  syncWithQueryString()
  {
    const params = this.parseQueryString();
    if (JSON.stringify(params) != JSON.stringify(this.videoSearchParameters))
    {
      this.videoSearchParameters = params;
      this.notifyParametersChanged();
    }
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
  getVideos(): Observable<VideoSearchResultDTO>
  {
    let urlParams = new HttpParams();
    
    if (!this.videoSearchParameters)
    {
      this.videoSearchParameters =
      {
        text: null,
        sort: null,
        timeFilter: null,
        lengthFilter: null,
        batchSize: this.DEFAULT_BATCH_SIZE,
        uploaderIdFilter: null,
        uploaderAlias: null
      };
    }
    if (this.videoSearchParameters.text)
      urlParams = urlParams.set("text", this.videoSearchParameters.text);
    if (this.videoSearchParameters.sort)
      urlParams = urlParams.set("sort", this.videoSearchParameters.sort);
    if (this.videoSearchParameters.lengthFilter)
      urlParams = urlParams.set("lengthFilter", this.videoSearchParameters.lengthFilter);
    if (this.videoSearchParameters.timeFilter)
      urlParams = urlParams.set("timeFilter", this.videoSearchParameters.timeFilter);
    if (this.videoSearchParameters.uploaderIdFilter)
      urlParams = urlParams.set("uploaderIdFilter", this.videoSearchParameters.uploaderIdFilter);
    urlParams = urlParams.set("batchSize", this.videoSearchParameters.batchSize.toString());
    return this.searchVideosByQueryString(urlParams.toString());
  }
  getSearchControls(): Observable<SearchControlsDTO>
  {
    const result = this.client.get<SearchControlsDTO>("Videos/VideosSearch/controls");
    return result;
  }
  private searchVideosByQueryString(parameters: string): Observable<VideoSearchResultDTO>
  {
    if (!parameters.trim())
    {
      throw new Error("Empty parameters string provided.");
    }
    const result = this.client.post<VideoSearchResultRawDTO>("Videos/VideosSearch/videos?" + parameters, this.meta).pipe(
      map(response =>
      {
        this.meta.meta = response.meta;
        return new VideoSearchResultDTO(response);
      })
    );
    return result;
  }
  
  private parseQueryString(): VideoSearchParametersDTO | null
  {
    const textParam = <string>this.activatedRoute.snapshot.queryParams["text"]?.trim();
    const sortParam = <string>this.activatedRoute.snapshot.queryParams["sort"]?.trim();
    const timeFilterParam = <string>this.activatedRoute.snapshot.queryParams["timeFilter"]?.trim();
    const lengthFilterParam = <string>this.activatedRoute.snapshot.queryParams["lengthFilter"]?.trim();
    const uploaderIdFilterParam = <string>this.activatedRoute.snapshot.queryParams["uploaderIdFilter"]?.trim();
    const uploaderAliasParam = <string>this.activatedRoute.snapshot.queryParams["uploaderAlias"]?.trim();
    const batchSizeParam = <number>this.activatedRoute.snapshot.queryParams["batchSize"]?.trim();
    const params: VideoSearchParametersDTO = {
      text: textParam,
      sort: sortParam,
      timeFilter: timeFilterParam,
      lengthFilter: lengthFilterParam,
      batchSize: batchSizeParam ? batchSizeParam : this.DEFAULT_BATCH_SIZE,
      uploaderIdFilter: uploaderIdFilterParam,
      uploaderAlias: uploaderAliasParam
    };
    return params;
  }
  private notifyParametersChanged()
  {
    this.videoSearchParameters$.next(this.videoSearchParameters);
  }

}
