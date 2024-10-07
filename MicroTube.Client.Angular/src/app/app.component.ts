import { AfterViewInit, Component, OnDestroy, ViewChild } from "@angular/core";
import { AuthManager } from "./services/auth/AuthManager";
import { MatMenuTrigger } from "@angular/material/menu";
import { VideoService } from "./services/videos/VideoService";
import { map, Subscription } from "rxjs";
import {NavigationEnd, Router } from "@angular/router";
import { VideoSearchService } from "./services/videos/VideoSearchService";
import { SuggestionSearchBarComponent } from "./utility-components/suggestion-search-bar/suggestion-search-bar.component";
import { MatDialog } from "@angular/material/dialog";
import { AuthPopupComponent } from "./auth/auth-popup/auth-popup.component";
import { SessionManager } from "./services/auth/SessionManager";
import { VideoSearchQueryStringReader } from "./services/query-string-processing/VideoSearchQueryStringReader";
import { QueryStringBuilder } from "./services/query-string-processing/QueryStringBuilder";

@Component({
  selector: "app-root",
  templateUrl: "./app.component.html",
  styleUrls: ["./app.component.scss"]
})
export class AppComponent implements AfterViewInit, OnDestroy
{
  title = "app";
  private readonly searchService: VideoSearchService;
  private readonly searchQueryReader: VideoSearchQueryStringReader;
  private readonly queryBuilder: QueryStringBuilder;
  readonly router: Router;
  readonly authManager: AuthManager;
  readonly videoService: VideoService;
  private readonly dialog: MatDialog;
  searchSubscription: Subscription | null = null;
  @ViewChild(MatMenuTrigger)
  signOutMenuTrigger!: MatMenuTrigger;
  @ViewChild(SuggestionSearchBarComponent)
  suggestionSearchBar!: SuggestionSearchBarComponent;
  videoSearchSuggestionsSubscription: Subscription | null = null;
  videoSearchSuggestionsSource: string[] = [];
  private routerSubscription: Subscription|null = null;
  constructor(
    authManager: AuthManager,
    videoService: VideoService,
    router: Router,
    searchService: VideoSearchService,
    searchQueryReader: VideoSearchQueryStringReader,
    queryBuilder: QueryStringBuilder,
    dialogue: MatDialog, private readonly sessionManager: SessionManager)
  {
    this.searchService = searchService;
    this.router = router;
    this.videoService = videoService;
    this.authManager = authManager;
    this.searchQueryReader = searchQueryReader;
    this.dialog = dialogue;
    this.queryBuilder = queryBuilder;
  }
  ngOnDestroy(): void
  {
    this.routerSubscription?.unsubscribe();
  }
  ngAfterViewInit(): void {
    this.routerSubscription = this.router.events.subscribe(value =>
      {
        if (value instanceof NavigationEnd)
        {
          this.syncSearchBarText();
        }
      });
      this.syncSearchBarText();
    }
  isUserEmailConfirmed()
  {
    return this.authManager.jwtSignedInUser$.value?.isEmailConfirmed;
  }
  searchVideo(searchText: string | null)
  {
    const trimmedSearchText = searchText?.trim() ?? null;
    this.queryBuilder.setValue("text", trimmedSearchText);
    this.queryBuilder.navigate("/");
  }
  updateSearchSuggestions(text: string | null)
  {
    if (!text || !text.trim())
    {
      this.videoSearchSuggestionsSource.length = 0;
      return;
    }
    this.videoSearchSuggestionsSubscription = this.searchService.getSearchSuggestions(text)
      .pipe(
        map(res => res.map(_ => _.text)))
      .subscribe(suggestions =>
      {
        this.videoSearchSuggestionsSource.length = 0;
        suggestions.forEach(_ => this.videoSearchSuggestionsSource.push(_));
      });
  }
  openSignIn()
  {
    if (!this.authManager.isSignedIn())
    {
      this.dialog.open(AuthPopupComponent);
    }
  }
  syncSearchBarText()
  {
    const searchParams = this.searchQueryReader.readSearchParameters();
    this.suggestionSearchBar.inputControl.setValue(searchParams.text, { emitEvent: false });
  }
}
