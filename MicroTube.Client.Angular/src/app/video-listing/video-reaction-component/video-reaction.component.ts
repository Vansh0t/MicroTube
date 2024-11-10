import { Component, Input, OnDestroy, OnInit } from "@angular/core";
import { AuthPopupComponent } from "../../auth/auth-popup/auth-popup.component";
import { VideoDto } from "../../data/Dto/VideoDto";
import { Subscription } from "rxjs";
import { AuthManager } from "../../services/auth/AuthManager";
import { MatDialog } from "@angular/material/dialog";
import { JWTUser } from "../../services/auth/JWTUser";
import { LikeDislikeReactionType } from "../../services/ReactionTypes";
import { LikeDislikeReactionService } from "../../services/reactions/LikeDislikeReactionService";
import { LikeDislikeReactionDto } from "../../data/Dto/LikeDislikeReactionDto";
import { LikeDislikeReactionsAggregator } from "../../services/reactions/LikeDislikeReactionsAggregator";

@Component({
  selector: "video-reaction",
  templateUrl: "./video-reaction.component.html",
  styleUrl: "./video-reaction.component.css"
})
export class VideoReactionComponent implements OnInit, OnDestroy
{
  private VIDEO_REACTION_TARGET_KEY = "video";
  @Input() video: VideoDto | null = null;
  private readonly auth: AuthManager;
  private readonly dialog: MatDialog;
  private readonly reactionService: LikeDislikeReactionService;
  private readonly reactionsAggregator: LikeDislikeReactionsAggregator;
  private userAuthStateSubscription: Subscription | null = null;
  private userReactSubscription: Subscription | null = null;
  private userGetReactionSubscription: Subscription | null = null;
  get isDisiked()
  {
    return this.currentReactionType == LikeDislikeReactionType.Dislike;
  }
  get isLiked()
  {
    return this.currentReactionType == LikeDislikeReactionType.Like;
  }
  get currentReactionType()
  {
    return this.userCurrentReaction ? this.userCurrentReaction.reactionType : LikeDislikeReactionType.None;
  }
  userCurrentReaction: LikeDislikeReactionDto | null = null;
  constructor(auth: AuthManager, dialog: MatDialog, reactionService: LikeDislikeReactionService, reactionsAggregator: LikeDislikeReactionsAggregator)
  {
    this.dialog = dialog;
    this.auth = auth;
    this.reactionService = reactionService;
    this.reactionsAggregator = reactionsAggregator;
  }
  ngOnInit(): void
  {
    this.userAuthStateSubscription = this.auth.jwtSignedInUser$.subscribe(this.onUserAuthStateChanged.bind(this));
  }
  ngOnDestroy(): void {
    this.userAuthStateSubscription?.unsubscribe();
  }

  likeVideo()
  {
    if (!this.auth.isSignedIn())
    {
      this.dialog.open(AuthPopupComponent);
      return;
    }
    if (this.video)
    {
      this.userReactSubscription?.unsubscribe();
      const targetReactionType = this.currentReactionType == LikeDislikeReactionType.Like ? LikeDislikeReactionType.None : LikeDislikeReactionType.Like;
      this.userReactSubscription = this.reactionService
        .setReaction(this.VIDEO_REACTION_TARGET_KEY, this.video.id, targetReactionType)
        .subscribe(this.onReactionUpdate.bind(this));
    }
  }
  dislikeVideo()
  {
    if (!this.auth.isSignedIn())
    {
      this.dialog.open(AuthPopupComponent);
      return;
    }
    if (this.video)
    {
      this.userReactSubscription?.unsubscribe();
      const targetReactionType = this.currentReactionType == LikeDislikeReactionType.Dislike ? LikeDislikeReactionType.None : LikeDislikeReactionType.Dislike;
      this.userReactSubscription = this.reactionService
        .setReaction(this.VIDEO_REACTION_TARGET_KEY, this.video.id, targetReactionType)
        .subscribe(this.onReactionUpdate.bind(this));
    }
  }
  private onReactionUpdate(reaction: LikeDislikeReactionDto | null)
  {
    if (this.video && reaction)
    {
      this.video.reactionsAggregation = this.reactionsAggregator.aggregate(this.video.reactionsAggregation, this.currentReactionType, reaction.reactionType);
    }
    this.onReaction(reaction);
  }
  private getUserReaction()
  {
    if (this.video)
    {
      this.userGetReactionSubscription?.unsubscribe();
      this.userGetReactionSubscription = this.reactionService.getReaction(this.VIDEO_REACTION_TARGET_KEY, this.video.id)
        .subscribe({
          next: this.onReaction.bind(this),
          error: () => this.onReaction(null)
        });
    }
  }
  private onUserAuthStateChanged(user: JWTUser | null)
  {
    if (this.video && user && user.isEmailConfirmed)
    {
      this.getUserReaction();
    }
  }
  private onReaction(reaction: LikeDislikeReactionDto | null)
  {
    this.userCurrentReaction = reaction;
  }
}
