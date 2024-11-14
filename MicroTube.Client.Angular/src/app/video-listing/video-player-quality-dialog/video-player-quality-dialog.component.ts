import { Component, Inject } from "@angular/core";
import { QualityOption } from "../ngx-player/ngx-player.component";
import { MAT_DIALOG_DATA, MatDialogRef } from "@angular/material/dialog";

@Component({
  selector: "app-video-player-quality-dialog",
  templateUrl: "./video-player-quality-dialog.component.html",
  styleUrl: "./video-player-quality-dialog.component.css"
})
export class VideoPlayerQualityDialogComponent
{
  readonly dialogRef: MatDialogRef<VideoPlayerQualityDialogComponent>;
  data: { selectedQuality: QualityOption, qualityOptions: QualityOption[] | null; };
  constructor(dialogRef: MatDialogRef<VideoPlayerQualityDialogComponent>,
    @Inject(MAT_DIALOG_DATA) data: { selectedQuality: QualityOption, qualityOptions: QualityOption[] | null },
  )
  {
    this.dialogRef = dialogRef;
    this.data = data;
  }

  selectQuality(qualityOption: QualityOption): void
  {
    this.dialogRef.close(qualityOption);
  }
}
