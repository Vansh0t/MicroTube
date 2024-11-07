import { Component, Input, OnDestroy, OnInit } from "@angular/core";
import { ICommentSearchService } from "../../services/ICommentSearchService";
import { Subscription } from "rxjs";
import { CommentSortType } from "../../services/SortTypes";
import { CommentSearchResultDTO } from "../../data/DTO/CommentSearchResultDTO";
import { HttpErrorResponse } from "@angular/common/http";
import { CommentDTO } from "../../data/DTO/CommentDTO";

@Component({
  selector: "comments-area",
  templateUrl: "./comments-area.component.html",
  styleUrl: "./comments-area.component.scss"
})
export class CommentsAreaComponent implements OnInit, OnDestroy
{
  readonly COMMENTS_BATCH_SIZE = 20;
  @Input() searchService: ICommentSearchService | undefined;
  @Input() targetId: string | undefined;
  comments: CommentDTO[] | null = null;
  get isLoading()
  {
    return this.commentsLoadingSubscription != null;
  }
  endOfDataReached: boolean = false;

  private commentsLoadingSubscription: Subscription | null = null;
  
  ngOnInit(): void
  {
    if (!this.searchService || !this.targetId)
    {
      throw new Error("Search service or targetId is not set.");
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
    if (this.isLoading || !this.targetId || !this.searchService)
    {
      return;
    }
    this.commentsLoadingSubscription?.unsubscribe();
    this.commentsLoadingSubscription = null;
    this.commentsLoadingSubscription = this.searchService.getComments(this.targetId, { batchSize: this.COMMENTS_BATCH_SIZE, sortType: CommentSortType.Top })
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
  private onNextBatchReceived(result: CommentSearchResultDTO)
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