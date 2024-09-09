import { Component, Input, OnDestroy, OnInit } from "@angular/core";
import { AuthPopupComponent } from "../../auth/auth-popup/auth-popup.component";
import { VideoDTO } from "../../data/DTO/VideoDTO";
import { Subscription } from "rxjs";
import { AuthManager } from "../../services/auth/AuthManager";
import { MatDialog } from "@angular/material/dialog";
import { VideoReactionType, VideoService } from "../../services/videos/VideoService";
import { JWTUser } from "../../services/auth/JWTUser";
import { UserVideoReactionDTO } from "../../data/DTO/UserVideoReactionDTO";

@Component({
  selector: "video-reaction",
  templateUrl: "./video-reaction.component.html",
  styleUrl: "./video-reaction.component.css"
})
export class LikeComponent implements OnInit, OnDestroy
{
  @Input() video: VideoDTO | null = null;
  private readonly auth: AuthManager;
  private readonly dialog: MatDialog;
  private readonly videoService: VideoService;
  private userAuthStateSubscription: Subscription | null = null;
  private userReactSubscription: Subscription | null = null;
  private userGetReactionSubscription: Subscription | null = null;
  get isDisiked()
  {
    return this.currentReactionType == VideoReactionType.Dislike;
  }
  get isLiked()
  {
    return this.currentReactionType == VideoReactionType.Like;
  }
  get currentReactionType()
  {
    return this.userCurrentReaction ? this.userCurrentReaction.reactionType : VideoReactionType.None;
  }
  userCurrentReaction: UserVideoReactionDTO | null = null;
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
      const targetReactionType = this.currentReactionType == VideoReactionType.Like ? VideoReactionType.None : VideoReactionType.Like;
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
      const targetReactionType = this.currentReactionType == VideoReactionType.Dislike ? VideoReactionType.None : VideoReactionType.Dislike;
      this.userReactSubscription = this.videoService.react(this.video.id, targetReactionType).subscribe(this.onVideoReacted.bind(this));
    }
  }
  private onVideoReacted(reaction: UserVideoReactionDTO | null)
  {
    if (!this.video)
      return;
    const prevReactionType = this.currentReactionType;
    this.onReaction(reaction);
    if (prevReactionType == VideoReactionType.None)
    {
      if (this.currentReactionType == VideoReactionType.Like)
        this.video.likes++;
      else if (this.currentReactionType == VideoReactionType.Dislike)
        this.video.dislikes++;
    }
    else if (prevReactionType == VideoReactionType.Like)
    {
      if (this.currentReactionType == VideoReactionType.Dislike)
      {
        this.video.likes--;
        this.video.dislikes++;
      }
      else if (this.currentReactionType == VideoReactionType.None)
      {
        this.video.likes--;
      }
    }
    else if (prevReactionType == VideoReactionType.Dislike)
    {
      if (this.currentReactionType == VideoReactionType.Like)
      {
        this.video.likes++;
        this.video.dislikes--;
      }
      else if (this.currentReactionType == VideoReactionType.None)
      {
        this.video.dislikes--;
      }
    }
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
  private onReaction(reaction: UserVideoReactionDTO | null)
  {
    this.userCurrentReaction = reaction;
  }
}
