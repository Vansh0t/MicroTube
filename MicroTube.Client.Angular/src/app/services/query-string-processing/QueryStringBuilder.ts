import { Injectable } from "@angular/core";
import { ActivatedRoute, Params, Router } from "@angular/router";

@Injectable({
  providedIn: "root"
})
export class QueryStringBuilder
{
  private readonly activatedRoute: ActivatedRoute;
  private readonly router: Router;
  private queryParams: Params = {};

  constructor(activatedRoute: ActivatedRoute, router: Router)
  {
    this.router = router;
    this.activatedRoute = activatedRoute;
  }
  setValue(key: string, value: string | number | boolean | null)
  {
    if (!value)
    {
      delete this.queryParams[key];
    }
    else
    {
      this.queryParams[key] = value;
    }
  }
  navigate(url: string)
  {
    this.router.navigate(
      [url],
      {
        queryParams: this.queryParams
      }
    );
  }
  reset()
  {
    this.queryParams = {};
  }
}
