import { Component, ViewChild } from "@angular/core";
import { AuthManager } from "./services/auth/AuthManager";
import { MatMenuTrigger } from "@angular/material/menu";
import { SessionManager } from "./services/auth/SessionManager";
import { VideoService } from "./services/videos/VideoService";
import { map, Observable } from "rxjs";

@Component({
  selector: "app-root",
  templateUrl: "./app.component.html",
  styleUrls: ["./app.component.scss"]
})
export class AppComponent {
  title = "app";
  private readonly sessionManager: SessionManager;
  readonly authManager: AuthManager;
  readonly videoService: VideoService;
  @ViewChild(MatMenuTrigger)
  signOutMenuTrigger!: MatMenuTrigger;
  videoSearchSuggestions$: Observable<string[]> | null = null;
  constructor(authManager: AuthManager, sessionManager: SessionManager, videoService: VideoService)
  {
    this.videoService = videoService;
    this.authManager = authManager;
    this.sessionManager = sessionManager;
    console.log(this.videoService);
    console.log(this.authManager);
  }
  isUserEmailConfirmed()
  {
    return this.authManager.jwtSignedInUser$.value?.isEmailConfirmed;
  }
  searchVideo(searchText: string | null)
  {
    console.log(searchText);
  }
  updateSearchSuggestions(text: string | null)
  {
    if (!text || !text.trim())
    {
      this.videoSearchSuggestions$ = new Observable<string[]>();
      return;
    }
    console.log(this.videoService);
    console.log(this.authManager);
    this.videoSearchSuggestions$ = this.videoService.getSearchSuggestions(text)
      .pipe(
        map(res => res.map(_ => _.text)));
    this.videoSearchSuggestions$.subscribe(_ => console.log(_));
  }
}
