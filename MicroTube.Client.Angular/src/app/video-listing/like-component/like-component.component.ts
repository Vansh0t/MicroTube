import { Component, Input, OnDestroy, OnInit } from "@angular/core";
import { AuthPopupComponent } from "../../auth/auth-popup/auth-popup.component";
import { VideoDTO } from "../../data/DTO/VideoDTO";
import { Subscription } from "rxjs";
import { VideoLikeDTO } from "../../data/DTO/VideoLikeDTO";
import { AuthManager } from "../../services/auth/AuthManager";
import { MatDialog } from "@angular/material/dialog";
import { VideoService } from "../../services/videos/VideoService";
import { JWTUser } from "../../services/auth/JWTUser";
import { VideoDislikeDTO } from "../../data/DTO/VideoDislikeDTO";

@Component({
  selector: "like-component",
  templateUrl: "./like-component.component.html",
  styleUrl: "./like-component.component.css"
})
export class LikeComponent implements OnInit, OnDestroy
{
  @Input() video: VideoDTO | null = null;
  private readonly auth: AuthManager;
  private readonly dialog: MatDialog;
  private readonly videoService: VideoService;
  private userAuthStateSubscription: Subscription | null = null;
  private userLikeSubscription: Subscription | null = null;
  private userDislikeSubscription: Subscription | null = null;
  userLike: VideoLikeDTO | null = null;
  userDislike: VideoDislikeDTO | null = null;
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
    this.userLikeSubscription?.unsubscribe();
    this.userDislikeSubscription?.unsubscribe();
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
      this.userLikeSubscription?.unsubscribe();
      if (!this.userLike)
        this.userLikeSubscription = this.videoService.likeVideo(this.video.id).subscribe(this.onVideoLiked.bind(this));
      else
        this.userLikeSubscription = this.videoService.unlikeVideo(this.video.id).subscribe({ next: () => this.onVideoUnliked() });
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
      this.userDislikeSubscription?.unsubscribe();
      if (!this.userDislike)
        this.userDislikeSubscription = this.videoService.dislikeVideo(this.video.id).subscribe(this.onVideoDisliked.bind(this));
      else
        this.userDislikeSubscription = this.videoService.unlikeVideo(this.video.id).subscribe({ next: () => this.onVideoUnliked() });
    }
  }
  private onVideoLiked(like: VideoLikeDTO | null)
  {
    this.onLike(like);
    if (!this.video)
      return;
    this.video.likes++;
    if (this.userDislike)
    {
      this.onDislike(null);
      this.video.dislikes--;
    }
  }
  private onVideoDisliked(dislike: VideoDislikeDTO | null)
  {
    this.onDislike(dislike);
    if (!this.video)
      return;
    this.video.dislikes++;
    if (this.userLike)
    {
      this.onLike(null);
      this.video.likes--;
    }
  }
  private onVideoUnliked()
  {
    this.onLike(null);
    if (!this.video)
      return;
    this.video.likes--;
  }
  private onLike(like: VideoLikeDTO | null)
  {
    this.userLike = like;
  }
  private onDislike(dislike: VideoDislikeDTO | null)
  {
    this.userDislike = dislike;
  }
  private fetchUserLike()
  {
    if (this.video)
    {
      this.userLikeSubscription?.unsubscribe();
      this.userLikeSubscription = this.videoService.getVideoLike(this.video.id)
        .subscribe({
          next: this.onLike.bind(this),
          error: () => this.onLike(null)
        });
    }
  }
  private fetchUserDislike()
  {
    if (this.video)
    {
      this.userDislikeSubscription?.unsubscribe();
      this.userDislikeSubscription = this.videoService.getVideoDislike(this.video.id)
        .subscribe({
          next: this.onDislike.bind(this),
          error: () => this.onDislike(null)
        });
    }
  }
  private onUserAuthStateChanged(user: JWTUser | null)
  {
    if (this.video && user && user.isEmailConfirmed)
    {
      this.fetchUserLike();
      this.fetchUserDislike();
    }
  }
}
