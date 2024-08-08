import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { VideoCardComponent } from "./video-card/video-card.component";
import { MatCardModule } from "@angular/material/card";
import { RouterModule } from "@angular/router";
import { VideoListingMainComponent } from "./video-listing-main/video-listing-main.component";
import { VideoWatchComponent } from "./video-watch/video-watch.component";
import { VideoUploadComponent } from "./video-upload/video-upload.component";
import { MatFormFieldModule } from "@angular/material/form-field";
import { ReactiveFormsModule } from "@angular/forms";
import { MatInputModule } from "@angular/material/input";
import { MatIconModule } from "@angular/material/icon";
import { MaterialFileInputModule } from "ngx-custom-material-file-input";
import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";
import { MatButtonModule } from "@angular/material/button";
import { UploadProgressListComponent } from "./upload-progress-list/upload-progress-list.component";
import { MatTableModule } from "@angular/material/table";
import { MatPaginatorModule } from "@angular/material/paginator";
import { MatSortModule } from "@angular/material/sort";
import { MatMenuModule } from "@angular/material/menu";
import { MatOptionModule } from "@angular/material/core";
import { MatSelectModule } from "@angular/material/select";
import { NgxPlayerComponent } from "./ngx-player/ngx-player.component";


@NgModule({
  declarations: [
    VideoCardComponent,
    VideoListingMainComponent,
    VideoWatchComponent,
    VideoUploadComponent,
    UploadProgressListComponent
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
    MatButtonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatMenuModule,
    MatOptionModule,
    MatSelectModule,
    NgxPlayerComponent
  ],
  exports: [
    VideoCardComponent,
    VideoListingMainComponent,
    VideoWatchComponent,
    VideoUploadComponent,
    UploadProgressListComponent
  ]
})
export class VideoListingModule { }
