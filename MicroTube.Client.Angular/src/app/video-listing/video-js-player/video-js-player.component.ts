import { Component, ElementRef, Input, OnDestroy, OnInit, ViewChild } from "@angular/core";
import videojs from "video.js";
import Player from "video.js/dist/types/player";

@Component({
  selector: "video-js-player",
  templateUrl: "./video-js-player.component.html",
  styleUrls: ["./video-js-player.component.css"]
})
export class VideoJsPlayerComponent implements OnInit, OnDestroy
{
  @ViewChild("target", { static: true }) target: ElementRef | undefined;
  @Input() options: object | undefined;
  player: Player | undefined;
  
  ngOnInit(): void
  {
    if (this.target == null)
    {
      console.error("Player target is not set");
      return;
    }
    if (this.options == null)
    {
      console.error("Player options is not set");
      return;
    }
    this.player = videojs(this.target.nativeElement, this.options, () =>
    {
      console.log("Player ready");
    });
  }
  ngOnDestroy(): void
  {
    if (this.player != null)
      this.player.dispose();
  }
}
