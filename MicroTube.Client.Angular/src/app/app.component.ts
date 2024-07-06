import { Component, ViewChild } from "@angular/core";
import { AuthManager } from "./services/auth/AuthManager";
import { MatMenuTrigger } from "@angular/material/menu";
import { VideoService } from "./services/videos/VideoService";
import { map, Subscription } from "rxjs";
import { Router } from "@angular/router";

@Component({
  selector: "app-root",
  templateUrl: "./app.component.html",
  styleUrls: ["./app.component.scss"]
})
export class AppComponent {
  title = "app";
  readonly router: Router;
  readonly authManager: AuthManager;
  readonly videoService: VideoService;
  @ViewChild(MatMenuTrigger)
  signOutMenuTrigger!: MatMenuTrigger;
  videoSearchSuggestionsSubscription: Subscription | null = null;
  videoSearchSuggestionsSource: string[] = [];
  constructor(authManager: AuthManager, videoService: VideoService, router: Router)
  {
    this.router = router;
    this.videoService = videoService;
    this.authManager = authManager;
  }
  isUserEmailConfirmed()
  {
    return this.authManager.jwtSignedInUser$.value?.isEmailConfirmed;
  }
  searchVideo(searchText: string | null)
  {
    this.router.navigate(["/"], { queryParams: { videoSearch: searchText } });
  }
  updateSearchSuggestions(text: string | null)
  {
    if (!text || !text.trim())
    {
      this.videoSearchSuggestionsSource.length = 0;
      return;
    }
    this.videoSearchSuggestionsSubscription = this.videoService.getSearchSuggestions(text)
      .pipe(
        map(res => res.map(_ => _.text)))
      .subscribe(suggestions =>
      {
        this.videoSearchSuggestionsSource.length = 0;
        suggestions.forEach(_ => this.videoSearchSuggestionsSource.push(_));
      });
  }
}
