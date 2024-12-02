/* eslint-disable @typescript-eslint/no-explicit-any */
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { environment } from "../../../../environments/environment";
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { IS_NO_API_REQUEST } from "./InterceptorsShared";


@Injectable()
export class APIBaseURLInterceptor implements HttpInterceptor
{
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>>
  {
    if (req.context.get(IS_NO_API_REQUEST) === false)
    {
      const apiReq = req.clone({ url: `${environment.apiUrl}/${req.url}` });
      return next.handle(apiReq);
    }
    return next.handle(req);
  }
}
