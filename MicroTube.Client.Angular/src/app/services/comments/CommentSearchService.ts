import { Injectable } from "@angular/core";
import { Observable, map } from "rxjs";
import { HttpClient, HttpParams } from "@angular/common/http";
import { CommentSearchMetaDto } from "../../data/Dto/CommentSearchMetaDto";
import { CommentSearchParametersDto } from "../../data/Dto/CommentSearchParametersDto";
import { CommentSearchResultDto, CommentSearchResultRawDto } from "../../data/Dto/CommentSearchResultDto";

@Injectable({
  providedIn: "root"
})
export class CommentSearchService
{
  private meta: CommentSearchMetaDto = { meta: null };
  private readonly client: HttpClient;
  private readonly DEFAULT_BATCH_SIZE = 20;
  constructor(client: HttpClient)
  {
    this.client = client;
  }
  getComments(targetKey: string, targetId: string, params: CommentSearchParametersDto): Observable<CommentSearchResultDto>
  {
    let urlParams = new HttpParams();
    urlParams = urlParams.set("batchSize", params.batchSize ? params.batchSize.toString() : this.DEFAULT_BATCH_SIZE);
    urlParams = urlParams.set("sortType", params.sortType);
    return this.searchCommentsByQueryString(targetKey, targetId, urlParams.toString());
  }
 /* getSearchControls(): Observable<SearchControlsDto>
  {
    const result = this.client.get<SearchControlsDto>("videos/videossearch/controls");
    return result;
  }*/
  resetMeta()
  {
    this.meta.meta = null;
  }
  private searchCommentsByQueryString(targetKey: string, videoId: string, parameters: string): Observable<CommentSearchResultDto>
  {
    if (!parameters.trim())
    {
      throw new Error("Empty parameters string provided.");
    }
    const result = this.client.post<CommentSearchResultRawDto>(`comments/${targetKey}/${videoId}/get?${parameters}`, this.meta)
      .pipe(
        map(response =>
        {
          this.meta.meta = response.meta;
          return new CommentSearchResultDto(response);
        })
    );
    return result;
  }
}
