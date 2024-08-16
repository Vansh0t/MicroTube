import { Component, EventEmitter, HostListener, Input, OnDestroy, OnInit, Output } from "@angular/core";
import {VgApiService, VgCoreModule } from "@videogular/ngx-videogular/core";
import { VgControlsModule } from "@videogular/ngx-videogular/controls";
import { VgOverlayPlayModule } from "@videogular/ngx-videogular/overlay-play";
import { VgBufferingModule } from "@videogular/ngx-videogular/buffering";
import { FormsModule } from "@angular/forms";
import { MatInputModule } from "@angular/material/input";
import { MatSelectModule } from "@angular/material/select";
import { MatFormFieldModule } from "@angular/material/form-field";
import { NgxPlayerQualitySelectorComponent } from "./ngx-player-quality-select.component";
import { Subscription } from "rxjs";
import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";
@Component({
  standalone: true,
  selector: "ngx-player",
  templateUrl: "./ngx-player.component.html",
  styleUrls: ["./ngx-player.component.css"],
  imports: [
    VgCoreModule,
    VgControlsModule,
    VgOverlayPlayModule,
    VgBufferingModule,
    FormsModule,
    MatInputModule,
    MatSelectModule,
    MatFormFieldModule,
    NgxPlayerQualitySelectorComponent,
    MatProgressSpinnerModule
  ]
})
export class NgxPlayerComponent implements OnInit, OnDestroy
{
  @Output() onApi: EventEmitter<VgApiService> = new EventEmitter<VgApiService>();
  @Input() options!: NgxPlayerOptions;
  quality: QualityOption | null = null;
  api: VgApiService | null = null;
  isBuffering = false;
  get isMuted()
  {
    return this._isMuted;
  }
  private onNewMetadataLoadedSubscription: Subscription | null = null;
  private onBufferStateChangedSubscription: Subscription | null = null;
  private onVolumeChangedSubscription: Subscription | null = null;
  private playtimeToRestore = 0;
  private pausedStateToRestore = false;
  private volumeToRestore = -1;
  private _isMuted = false;
  private readonly QUICK_SEEK_TIME_SECONDS = 5;
  private readonly QUICK_VOLUME_PERCENT = 0.05;
  ngOnInit()
  {
    this.quality = this.options.qualityOptions[this.options.selectedQualityIndex];
  }
  ngOnDestroy(): void
  {
    this.onNewMetadataLoadedSubscription?.unsubscribe();
    this.onNewMetadataLoadedSubscription = null;
    this.onBufferStateChangedSubscription?.unsubscribe();
    this.onBufferStateChangedSubscription = null;
    this.onVolumeChangedSubscription?.unsubscribe();
    this.onVolumeChangedSubscription = null;
  }
  init(api: VgApiService)
  {
    this.onNewMetadataLoadedSubscription = api.getDefaultMedia().subscriptions.loadedMetadata
      .subscribe(this.restorePlay.bind(this));
    this.onBufferStateChangedSubscription = api.getDefaultMedia().subscriptions.bufferDetected
      .subscribe((isBuffering) => this.isBuffering = isBuffering);
    this.volumeToRestore = api.volume;
    this.onVolumeChangedSubscription = api.getDefaultMedia().subscriptions.volumeChange
      .subscribe(this.handleVolumeChange.bind(this));
    this.api = api;
    this.onApi.emit(api);
  }
  updateSource(quality: QualityOption)
  {
    if (!this.api)
      return;
    this.api.pause();
    this.playtimeToRestore = this.api.currentTime;
    this.pausedStateToRestore = this.api.state === "paused" ? true : false;
    this.quality = quality;
  }
  @HostListener("document:keydown", ["$event"])
  handlePlayerHotkeys(event: KeyboardEvent)
  {
    if (!(event.target instanceof Element) || !event.target.hasAttribute("allowVideoPlayerControls"))
    {
      return;
    } 
    if (this.api == null || !this.api.canPlay)
      return;
    if (event.key === " ")
    {
      event.preventDefault();
      if (this.api.state === "paused")
      {
        this.api.play();
      }
      else
      {
        this.api.pause();
      }
    }
    else if (event.key === "ArrowLeft")
    {
      event.preventDefault();
      const seekTarget = Math.max(0, this.api.currentTime - this.QUICK_SEEK_TIME_SECONDS);
      this.api.seekTime(seekTarget);
    }
    else if (event.key === "ArrowRight")
    {
      event.preventDefault();
      const seekTarget = Math.min(this.api.duration, this.api.currentTime + this.QUICK_SEEK_TIME_SECONDS);
      this.api.seekTime(seekTarget);
    }
    else if (event.key === "ArrowUp")
    {
      event.preventDefault();
      this.api.volume = Math.min(1, this.api.volume + this.QUICK_VOLUME_PERCENT);
    }
    else if (event.key === "ArrowDown")
    {
      this.api.volume = Math.max(0, this.api.volume - this.QUICK_VOLUME_PERCENT);
    }
    else if (event.key === "m" || event.key === "M")
    {
      event.preventDefault();
      if (this._isMuted)
      {
        this.api.volume = this.volumeToRestore;
      }
      else
      {
        this.api.volume = 0;
      }
    }
    else if (event.key === "f" || event.key === "F")
    {
      event.preventDefault();
      this.api.fsAPI.toggleFullscreen();
    }
  }
  private restorePlay()
  {
    if (this.playtimeToRestore > 0 && this.api)
    {
      this.api.seekTime(this.playtimeToRestore);
      this.api.play();
      if (this.pausedStateToRestore)
        this.api.pause();
      this.playtimeToRestore = 0;
    }
  }
  private handleVolumeChange()
  {
    if (this.api == null)
      return;
    const volume = this.api.volume;
    if (volume === 0)
      this._isMuted = true;
    else
    {
      this.volumeToRestore = volume;
      this._isMuted = false;
    }
  }
}
export interface QualityOption
{
  label: string;
  height: number;
  sourceUrl: string;
  mediaType: string | null;
}
export interface NgxPlayerOptions
{
  qualityOptions: QualityOption[];
  selectedQualityIndex: number;
}
