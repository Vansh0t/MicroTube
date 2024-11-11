import { Component, Input, OnDestroy, OnInit } from "@angular/core";
import { CommentDto } from "../../data/Dto/CommentDto";
import { TimeFormatter } from "../../services/formatting/TimeFormatter";
import { DateTime } from "luxon";
import { LikeDislikeReactionService } from "../../services/reactions/LikeDislikeReactionService";
import { LikeDislikeReactionDto } from "../../data/Dto/LikeDislikeReactionDto";
import { LikeDislikeReactionType } from "../../services/ReactionTypes";
import { Subscription } from "rxjs";
import { FakeLikeDislikeReactionsAggregator } from "../../services/reactions/FakeLikeDislikeReactionsAggregator";
import { AuthManager } from "../../services/auth/AuthManager";
import { MatDialog } from "@angular/material/dialog";
import { AuthPopupComponent } from "../../auth/auth-popup/auth-popup.component";
import { ConfirmPopupDialogComponent, ConfirmationImportance } from "../../utility-components/confirm-popup-dialog/confirm-popup-dialog.component";
import { CommentingService } from "../../services/comments/CommentingService";
import { MatSnackBar } from "@angular/material/snack-bar";
import { CommentPopupComponent } from "../comment-popup/comment-popup.component";

@Component({
  selector: "comment",
  templateUrl: "./comment.component.html",
  styleUrl: "./comment.component.css"
})
export class CommentComponent implements OnInit, OnDestroy
{
  private readonly REACTION_TARGET_KEY = "comment";
  @Input() comment: CommentDto | undefined;
  @Input() commentTargetKey: string | undefined;
  @Input() commentTargetId: string | undefined;
  private readonly authManager: AuthManager;
  private readonly dialog: MatDialog;
  private readonly commentingService: CommentingService;
  private editSubscription: Subscription | null = null;

  get currentReactionType()
  {
     return this.comment?.reaction ? this.comment.reaction.reactionType : LikeDislikeReactionType.None;
  }
  get isLiked()
  {
    return this.currentReactionType == LikeDislikeReactionType.Like;
  }
  get isDisliked()
  {
    return this.currentReactionType == LikeDislikeReactionType.Dislike;
  }
  get signedInUserId()
  {
    if (this.authManager.isSignedIn())
    {
      return this.authManager.jwtSignedInUser$.value!.userId;
    }
    return null;
  }
  private reactionSubscription: Subscription | null = null;
  private deletionSubscription: Subscription | null = null;
  private readonly timeFormatter: TimeFormatter;
  private readonly reactionService: LikeDislikeReactionService;
  private readonly aggregator: FakeLikeDislikeReactionsAggregator;
  private readonly snackbar: MatSnackBar;
  constructor(
    timeFormatter: TimeFormatter,
    reactionService: LikeDislikeReactionService,
    aggregator: FakeLikeDislikeReactionsAggregator,
    authManager: AuthManager,
    dialog: MatDialog,
    commentingService: CommentingService,
    snackbar: MatSnackBar)
  {
    this.timeFormatter = timeFormatter;
    this.reactionService = reactionService;
    this.aggregator = aggregator;
    this.authManager = authManager;
    this.dialog = dialog;
    this.commentingService = commentingService;
    this.snackbar = snackbar;
  }
  ngOnDestroy(): void
  {
    this.reactionSubscription?.unsubscribe();
    this.reactionSubscription = null;
    this.editSubscription?.unsubscribe();
    this.editSubscription = null;
  }
  ngOnInit(): void
  {
    if (!this.comment || !this.commentTargetKey)
    {
      throw new Error("comment and targetKey are required");
    }
  }
  getFormattedTime()
  {
    if (!this.comment)
    {
      return "";
    }
    const uploadTimeLocal = this.comment.time.toLocal();
    const nowLocal = DateTime.local();
    return this.timeFormatter.getUserFriendlyTimeDifference(uploadTimeLocal, nowLocal);
  }
  likeComment()
  {
    if (!this.authManager.isSignedIn())
    {
      this.dialog.open(AuthPopupComponent);
      return;
    }
    if (!this.comment)
    {
      return;
    }
    this.reactionSubscription?.unsubscribe();
    this.reactionSubscription = null;
    const targetReaction = this.currentReactionType == LikeDislikeReactionType.Like ? LikeDislikeReactionType.None : LikeDislikeReactionType.Like;
    this.reactionSubscription = this.reactionService.setReaction(this.REACTION_TARGET_KEY, this.comment.id, targetReaction)
      .subscribe({
        next: this.onReaction.bind(this),
        error: console.error
      });
  }
  dislikeComment()
  {
    if (!this.comment)
    {
      return;
    }
    this.reactionSubscription?.unsubscribe();
    this.reactionSubscription = null;
    const targetReaction = this.currentReactionType == LikeDislikeReactionType.Dislike ? LikeDislikeReactionType.None : LikeDislikeReactionType.Dislike;
    this.reactionSubscription = this.reactionService.setReaction(this.REACTION_TARGET_KEY, this.comment.id, targetReaction)
      .subscribe({
        next: this.onReaction.bind(this),
        error: console.error
      });
  }
  openDeleteConfirmation()
  {
    this.dialog.open(ConfirmPopupDialogComponent, { data: { info: "The comment will be deleted, this action cannot be undone. Proceed?", importance: ConfirmationImportance.High } })
      .afterClosed().subscribe((shouldProceed: boolean) =>
      {
        if (shouldProceed)
        {
          this.deleteComment();
        }
      });
  }
  startCommentEdit()
  {
    if (!this.comment)
    {
      return;
    }
    this.editSubscription?.unsubscribe();
    this.editSubscription = this.dialog.open(CommentPopupComponent, {
      data: {
        targetId: this.commentTargetId,
        commentId: this.comment.id,
        userAlias: this.comment.userAlias,
        commentTargetKey: this.commentTargetKey,
        editMode: true,
        content: this.comment.content
      }
    }).afterClosed().subscribe((comment: CommentDto) =>
    {
      if (!this.comment)
      {
        return;
      }
      this.comment.content = comment.content;
      this.comment.edited = comment.edited;
    }
    );
  }
  private onReaction(reaction: LikeDislikeReactionDto)
  {
    if (!this.comment)
    {
      return;
    }
    if (this.comment.reactionsAggregation)
    {
      const prevReaction = this.currentReactionType;
      this.comment.reactionsAggregation = this.aggregator.aggregate(this.comment.reactionsAggregation, prevReaction, reaction.reactionType);
    }
    this.comment.reaction = reaction;
  }
  private deleteComment()
  {
    if (!this.comment || !this.commentTargetKey)
    {
      return;
    }
    this.deletionSubscription?.unsubscribe();
    this.deletionSubscription = this.commentingService.deleteComment(this.commentTargetKey, this.comment.id)
      .subscribe({
        next: () =>
        {
          this.snackbar.open("Comment was deleted.", undefined, {duration: 3000});
          if (this.comment)
          {
            this.comment.deleted = true;
          }
        },
        error: (error) => { this.snackbar.open("Failed to delete the comment. " + error.error); }
      });
  }
} 
