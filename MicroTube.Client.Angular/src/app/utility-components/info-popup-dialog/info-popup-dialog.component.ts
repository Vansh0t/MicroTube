import { Component, Inject } from "@angular/core";
import { MAT_DIALOG_DATA, MatDialogRef } from "@angular/material/dialog";

@Component({
  selector: "info-popup-dialog",
  templateUrl: "./info-popup-dialog.component.html",
  styleUrls: ["./info-popup-dialog.component.css"]
})
export class InfoPopupDialogComponent
{
  readonly info: string;
  readonly dialogRef: MatDialogRef<InfoPopupDialogComponent>;
  constructor(@Inject(MAT_DIALOG_DATA) data: { info: string; }, dialogRef: MatDialogRef<InfoPopupDialogComponent>)
  {
    this.info = data.info;
    this.dialogRef = dialogRef;
  }
  close()
  {
    this.dialogRef.close();
  }
}
