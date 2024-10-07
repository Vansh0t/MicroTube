import { ActivatedRoute } from "@angular/router";
import { VideoSearchParametersDTO } from "../../data/DTO/VideoSearchDTO";
import { Injectable } from "@angular/core";

@Injectable({
  providedIn: "root"
})
export class VideoSearchQueryStringReader
{
  private readonly activatedRoute: ActivatedRoute;

  constructor(activatedRoute: ActivatedRoute)
  {
    this.activatedRoute = activatedRoute;
  }

  readSearchParameters(): VideoSearchParametersDTO
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
      batchSize: batchSizeParam,
      uploaderIdFilter: uploaderIdFilterParam,
      uploaderAlias: uploaderAliasParam
    };
    return params;
  }
}
