import { Component, Inject } from "@angular/core";
import { MAT_DIALOG_DATA, MatDialogRef } from "@angular/material/dialog";

@Component({
  selector: "confirm-popup-dialog",
  templateUrl: "./confirm-popup-dialog.component.html",
  styleUrls: ["./confirm-popup-dialog.component.css"]
})
export class ConfirmPopupDialogComponent
{
  readonly info: string;
  readonly dialogRef: MatDialogRef<ConfirmPopupDialogComponent>;
  readonly importance: string;
  constructor(@Inject(MAT_DIALOG_DATA) data: ConfirmDialogData, dialogRef: MatDialogRef<ConfirmPopupDialogComponent>)
  {
    this.info = data.info;
    this.dialogRef = dialogRef;
    this.importance = data.importance.toString();
  }
  confirm()
  {
    this.dialogRef.close(true);
  }
  cancel()
  {
    this.dialogRef.close(false);
  }
}
export enum ConfirmationImportance { Normal="Normal", High = "High"}
export interface ConfirmDialogData
{
  info: string;
  importance: ConfirmationImportance;
}
