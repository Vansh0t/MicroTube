import { Component, Input, OnDestroy, OnInit } from "@angular/core";
import { Subscription } from "rxjs";
import { CommentSortType } from "../../services/SortTypes";
import { CommentSearchResultDto } from "../../data/Dto/CommentSearchResultDto";
import { HttpErrorResponse } from "@angular/common/http";
import { CommentDto } from "../../data/Dto/CommentDto";
import { CommentSearchService } from "../../services/comments/CommentSearchService";

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
  comments: CommentDto[] | null = null;
  private readonly searchService: CommentSearchService;

  constructor(searchService: CommentSearchService)
  {
    this.searchService = searchService;
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
  }
  loadNextBatch()
  {
    if (this.isLoading || !this.targetId || !this.commentTargetKey)
    {
      return;
    }
    this.commentsLoadingSubscription?.unsubscribe();
    this.commentsLoadingSubscription = null;
    this.commentsLoadingSubscription = this.searchService.getComments(this.commentTargetKey, this.targetId, { batchSize: this.COMMENTS_BATCH_SIZE, sortType: CommentSortType.Top })
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
