import { Component, Input, OnDestroy, OnInit } from "@angular/core";
import { Subscription } from "rxjs";
import { CommentSortType } from "../../services/SortTypes";
import { CommentSearchResultDto } from "../../data/Dto/CommentSearchResultDto";
import { HttpErrorResponse } from "@angular/common/http";
import { CommentDto } from "../../data/Dto/CommentDto";
import { CommentSearchService } from "../../services/comments/CommentSearchService";
import { FormControl } from "@angular/forms";
import { AuthManager } from "../../services/auth/AuthManager";
import { MatDialog } from "@angular/material/dialog";
import { AuthPopupComponent } from "../../auth/auth-popup/auth-popup.component";
import { CommentPopupComponent } from "../comment-popup/comment-popup.component";

@Component({
  selector: "comments-area",
  templateUrl: "./comments-area.component.html",
  styleUrl: "./comments-area.component.scss"
})
export class CommentsAreaComponent implements OnInit, OnDestroy
{
  readonly COMMENTS_BATCH_SIZE = 20;
  @Input() commentTargetKey: string | undefined;
  @Input() targetId: string | undefined;
  @Input() commentsCount: number = 0;
  comments: CommentDto[] | null = null;
  sortControl: FormControl;
  private readonly searchService: CommentSearchService;
  private readonly authManager: AuthManager;
  private readonly dialog: MatDialog;
  private sortChangeSubscription: Subscription;
  constructor(searchService: CommentSearchService, authManager: AuthManager, dialog: MatDialog)
  {
    this.searchService = searchService;
    this.sortControl = new FormControl(CommentSortType.Top);
    this.sortChangeSubscription = this.sortControl.valueChanges
      .subscribe(this.onSortTypeChanged.bind(this));
    this.authManager = authManager;
    this.dialog = dialog;
  }
  get isLoading()
  {
    return this.commentsLoadingSubscription != null;
  }
  endOfDataReached: boolean = false;

  private commentsLoadingSubscription: Subscription | null = null;
  
  ngOnInit(): void
  {
    if (!this.commentTargetKey || !this.targetId)
    {
      throw new Error("commentTargetKey or targetId is not set.");
    }
    this.searchService.resetMeta();
    this.loadNextBatch();
  }
  ngOnDestroy(): void
  {
    this.commentsLoadingSubscription?.unsubscribe();
    this.commentsLoadingSubscription = null;
    this.sortChangeSubscription?.unsubscribe;
  }
  loadNextBatch()
  {
    if (this.isLoading || !this.targetId || !this.commentTargetKey)
    {
      return;
    }
    this.commentsLoadingSubscription?.unsubscribe();
    this.commentsLoadingSubscription = null;
    this.commentsLoadingSubscription = this.searchService.getComments(this.commentTargetKey, this.targetId, { batchSize: this.COMMENTS_BATCH_SIZE, sortType: this.sortControl.value })
      .subscribe({
        next: this.onNextBatchReceived.bind(this),
        error: this.onNextBatchFailed.bind(this)
      });
  }
  reloadComments()
  {
    this.commentsLoadingSubscription?.unsubscribe();
    this.commentsLoadingSubscription = null;
    this.comments = null;
    this.searchService!.resetMeta();
    this.endOfDataReached = false;
    this.loadNextBatch();
  }
  onSortTypeChanged()
  {
    this.reloadComments();
  }
  openCommentPopup()
  {
    if (!this.targetId)
    {
      return;
    }
    if (!this.authManager.isSignedIn())
    {
      this.dialog.open(AuthPopupComponent);
      return;
    }
    this.dialog.open(CommentPopupComponent, {
      data: {
        commentTargetKey: this.commentTargetKey,
        targetId: this.targetId,
        userAlias: this.authManager.jwtSignedInUser$.value?.publicUsername
      }
    })
      .afterClosed().subscribe((comment) =>
      {
        if (comment)
        {
          this.reloadComments();
        }
      });
  }
  private onNextBatchReceived(result: CommentSearchResultDto)
  {
    this.commentsLoadingSubscription?.unsubscribe();
    this.commentsLoadingSubscription = null;
    if (result.comments.length == 0)
    {
      this.endOfDataReached = true;
      return;
    }
    if (this.comments == null)
    {
      this.comments = result.comments;
    }
    else
    {
      this.comments = this.comments.concat(result.comments);
    }
  }
  private onNextBatchFailed(errorResponse: HttpErrorResponse)
  {
    this.commentsLoadingSubscription?.unsubscribe();
    this.commentsLoadingSubscription = null;
    console.error(errorResponse);
  }
}
