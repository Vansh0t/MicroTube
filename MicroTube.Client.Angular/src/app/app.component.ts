import { Component, OnInit, ViewChild } from "@angular/core";
import { AuthManager } from "./services/auth/AuthManager";
import { MatMenuTrigger } from "@angular/material/menu";
import { VideoService } from "./services/videos/VideoService";
import { map, Subscription } from "rxjs";
import { ActivatedRoute, Router } from "@angular/router";
import { VideoSearchService } from "./services/videos/VideoSearchService";
import { SuggestionSearchBarComponent } from "./utility-components/suggestion-search-bar/suggestion-search-bar.component";

@Component({
  selector: "app-root",
  templateUrl: "./app.component.html",
  styleUrls: ["./app.component.scss"]
})
export class AppComponent implements OnInit
{
  title = "app";
  private readonly searchService: VideoSearchService;
  private readonly activatedRoute: ActivatedRoute;
  readonly router: Router;
  readonly authManager: AuthManager;
  readonly videoService: VideoService;
  @ViewChild(MatMenuTrigger)
  signOutMenuTrigger!: MatMenuTrigger;
  @ViewChild(SuggestionSearchBarComponent)
  suggestionSearchBar!: SuggestionSearchBarComponent;
  videoSearchSuggestionsSubscription: Subscription | null = null;
  videoSearchSuggestionsSource: string[] = [];
  constructor(
    authManager: AuthManager,
    videoService: VideoService,
    router: Router,
    searchService: VideoSearchService,
    activatedRoute: ActivatedRoute)
  {
    this.searchService = searchService;
    this.router = router;
    this.videoService = videoService;
    this.authManager = authManager;
    this.activatedRoute = activatedRoute;
  }
    ngOnInit(): void {
      const textParam = <string>this.activatedRoute.snapshot.queryParams["text"]?.trim();
      if (textParam)
        this.suggestionSearchBar.inputControl.setValue(textParam, { emitEvent: false });
    }
  isUserEmailConfirmed()
  {
    return this.authManager.jwtSignedInUser$.value?.isEmailConfirmed;
  }
  searchVideo(searchText: string | null)
  {
    if (!searchText?.trim())
      this.router.navigate(["/"]);
    else
    {
      this.searchService.setText(searchText);
      this.searchService.search();
    }
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
