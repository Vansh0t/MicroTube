import { Component, Input, OnDestroy, OnInit } from "@angular/core";
import { AuthPopupComponent } from "../../auth/auth-popup/auth-popup.component";
import { VideoDto } from "../../data/Dto/VideoDto";
import { Subscription } from "rxjs";
import { AuthManager } from "../../services/auth/AuthManager";
import { MatDialog } from "@angular/material/dialog";
import { JWTUser } from "../../services/auth/JWTUser";
import { UserVideoReactionDto } from "../../data/Dto/UserVideoReactionDto";
import { LikeDislikeReactionType } from "../../services/ReactionTypes";
import { VideoService } from "../../services/videos/VideoService";

@Component({
  selector: "video-reaction",
  templateUrl: "./video-reaction.component.html",
  styleUrl: "./video-reaction.component.css"
})
export class LikeComponent implements OnInit, OnDestroy
{
  @Input() video: VideoDto | null = null;
  private readonly auth: AuthManager;
  private readonly dialog: MatDialog;
  private readonly videoService: VideoService;
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
  userCurrentReaction: UserVideoReactionDto | null = null;
  constructor(videoService: VideoService, auth: AuthManager, dialog: MatDialog)
  {
    this.dialog = dialog;
    this.auth = auth;
    this.videoService = videoService;
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
      this.userReactSubscription = this.videoService.react(this.video.id, targetReactionType).subscribe(this.onVideoReacted.bind(this));
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
      this.userReactSubscription = this.videoService.react(this.video.id, targetReactionType).subscribe(this.onVideoReacted.bind(this));
    }
  }
  private onVideoReacted(reaction: UserVideoReactionDto | null)
  {
    if (!this.video)
      return;
    //const prevReactionType = this.currentReactionType;
    this.onReaction(reaction);
    
  }
  private getUserReaction()
  {
    if (this.video)
    {
      this.userGetReactionSubscription?.unsubscribe();
      this.userGetReactionSubscription = this.videoService.getReaction(this.video.id)
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
  private onReaction(reaction: UserVideoReactionDto | null)
  {
    this.userCurrentReaction = reaction;
  }
}
