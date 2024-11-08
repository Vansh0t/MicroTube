import { Component, Input, OnDestroy, OnInit } from "@angular/core";
import { CommentDto } from "../../data/Dto/CommentDto";
import { TimeFormatter } from "../../services/formatting/TimeFormatter";
import { DateTime } from "luxon";
import { LikeDislikeReactionService } from "../../services/reactions/LikeDislikeReactionService";
import { LikeDislikeReactionDto } from "../../data/Dto/LikeDislikeReactionDto";
import { LikeDislikeReactionType } from "../../services/ReactionTypes";
import { Subscription } from "rxjs";
import { LikeDislikeReactionsAggregator } from "../../services/reactions/LikeDislikeReactionsAggregator";
import { AuthManager } from "../../services/auth/AuthManager";
import { MatDialog } from "@angular/material/dialog";
import { AuthPopupComponent } from "../../auth/auth-popup/auth-popup.component";

@Component({
  selector: "comment",
  templateUrl: "./comment.component.html",
  styleUrl: "./comment.component.css"
})
export class CommentComponent implements OnInit, OnDestroy
{
  private readonly REACTION_TARGET_KEY = "comment";
  @Input() comment: CommentDto | undefined;
  private readonly authManager: AuthManager;
  private readonly dialog: MatDialog;

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
  private reactionSubscription: Subscription | null = null;
  private readonly timeFormatter: TimeFormatter;
  private readonly reactionService: LikeDislikeReactionService;
  private readonly aggregator: LikeDislikeReactionsAggregator;
  constructor(timeFormatter: TimeFormatter, reactionService: LikeDislikeReactionService, aggregator: LikeDislikeReactionsAggregator, authManager: AuthManager, dialog: MatDialog)
  {
    this.timeFormatter = timeFormatter;
    this.reactionService = reactionService;
    this.aggregator = aggregator;
    this.authManager = authManager;
    this.dialog = dialog;
  }
  ngOnDestroy(): void
  {
    this.reactionSubscription?.unsubscribe();
    this.reactionSubscription = null;
  }
  ngOnInit(): void {
    if (!this.comment)
    {
      throw new Error("comment is required");
    }
    console.log(this.comment);
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
}
