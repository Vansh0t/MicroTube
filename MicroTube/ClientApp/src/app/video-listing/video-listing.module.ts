import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { VideoCardComponent } from "./video-card/video-card.component";
import { MatCardModule } from "@angular/material/card";
import { RouterModule } from "@angular/router";
import { VideoListingMainComponent } from "./video-listing-main/video-listing-main.component";
import { VideoWatchComponent } from "./video-watch/video-watch.component";
import { VideoJsPlayerComponent } from './video-js-player/video-js-player.component';


@NgModule({
  declarations: [
    VideoCardComponent,
    VideoListingMainComponent,
    VideoWatchComponent,
    VideoJsPlayerComponent
  ],
  imports: [
    CommonModule,
    MatCardModule,
    RouterModule
  ],
  exports: [
    VideoCardComponent,
    VideoListingMainComponent,
    VideoWatchComponent
  ]
})
export class VideoListingModule { }
