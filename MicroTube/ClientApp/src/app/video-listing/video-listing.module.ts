import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { VideoCardComponent } from "./video-card/video-card.component";
import { MatCardModule } from "@angular/material/card";
import { RouterModule } from "@angular/router";
import { VideoListingMainComponent } from "./video-listing-main/video-listing-main.component";
import { VideoWatchComponent } from "./video-watch/video-watch.component";
import { VideoJsPlayerComponent } from "./video-js-player/video-js-player.component";
import { VideoUploadComponent } from "./video-upload/video-upload.component";
import { MatFormFieldModule } from "@angular/material/form-field";
import { ReactiveFormsModule } from "@angular/forms";
import { MatInputModule } from "@angular/material/input";
import { MatIconModule } from "@angular/material/icon";
import { MaterialFileInputModule } from "ngx-custom-material-file-input";
import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";
import { MatButtonModule } from "@angular/material/button";


@NgModule({
  declarations: [
    VideoCardComponent,
    VideoListingMainComponent,
    VideoWatchComponent,
    VideoJsPlayerComponent,
    VideoUploadComponent
  ],
  imports: [
    CommonModule,
    MatCardModule,
    RouterModule,
    MatFormFieldModule,
    ReactiveFormsModule,
    MatInputModule,
    MatIconModule,
    MaterialFileInputModule,
    MatProgressSpinnerModule,
    MatButtonModule
  ],
  exports: [
    VideoCardComponent,
    VideoListingMainComponent,
    VideoWatchComponent,
    VideoUploadComponent
  ]
})
export class VideoListingModule { }
