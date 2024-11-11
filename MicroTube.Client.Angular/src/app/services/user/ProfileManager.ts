import { Observable } from "rxjs";
import { UserDto } from "../../data/Dto/UserDto";
import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";


@Injectable({
  providedIn: "root"
})
export class ProfileManager
{
  private readonly client: HttpClient;

  constructor(client: HttpClient)
  {
    this.client = client;
  }

  getUser(): Observable<UserDto>
  {
    const request = this.client.get<UserDto>("user/profile");
    return request;
  }
}
