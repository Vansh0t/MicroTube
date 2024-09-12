import { Observable } from "rxjs";
import { UserDTO } from "../../data/DTO/UserDTO";
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

  getUser(): Observable<UserDTO>
  {
    const request = this.client.get<UserDTO>("User/Profile");
    return request;
  }
}
