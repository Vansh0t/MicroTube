/* eslint-disable @typescript-eslint/no-explicit-any */
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";

@Injectable()
export class APIBaseURLInterceptor implements HttpInterceptor
{
  private readonly BASE_URL = "https://localhost:7146";
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>>
  {
    const apiReq = req.clone({ url: `${this.BASE_URL}/${req.url}` });
    return next.handle(apiReq);
  }
}
