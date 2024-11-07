import { Injectable } from "@angular/core";
import { Observable, map } from "rxjs";
import { HttpClient, HttpParams } from "@angular/common/http";
import { CommentSearchMetaDTO } from "../../data/DTO/CommentSearchMetaDTO";
import { CommentSearchParametersDTO } from "../../data/DTO/CommentSearchParametersDTO";
import { CommentSearchResultDTO, CommentSearchResultRawDTO } from "../../data/DTO/CommentSearchResultDTO";
import { ICommentSearchService } from "../ICommentSearchService";

@Injectable({
  providedIn: "root"
})
export class VideoCommentSearchService implements ICommentSearchService
{
  private meta: CommentSearchMetaDTO = { meta: null };
  private readonly client: HttpClient;
  private readonly DEFAULT_BATCH_SIZE = 20;
  constructor(client: HttpClient)
  {
    this.client = client;
  }
  getComments(targetId: string, params: CommentSearchParametersDTO): Observable<CommentSearchResultDTO>
  {
    let urlParams = new HttpParams();
    urlParams = urlParams.set("batchSize", params.batchSize ? params.batchSize.toString() : this.DEFAULT_BATCH_SIZE);
    urlParams = urlParams.set("sortType", params.sortType);
    return this.searchCommentsByQueryString(targetId, urlParams.toString());
  }
 /* getSearchControls(): Observable<SearchControlsDTO>
  {
    const result = this.client.get<SearchControlsDTO>("videos/videossearch/controls");
    return result;
  }*/
  resetMeta()
  {
    this.meta.meta = null;
  }
  private searchCommentsByQueryString(videoId: string, parameters: string): Observable<CommentSearchResultDTO>
  {
    if (!parameters.trim())
    {
      throw new Error("Empty parameters string provided.");
    }
    const result = this.client.post<CommentSearchResultRawDTO>(`comments/video/${videoId}/get?${parameters}`, this.meta)
      .pipe(
        map(response =>
        {
          this.meta.meta = response.meta;
          return new CommentSearchResultDTO(response);
        })
    );
    return result;
  }
}
