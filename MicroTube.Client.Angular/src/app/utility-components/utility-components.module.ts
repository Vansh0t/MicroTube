import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { MatDialogModule } from "@angular/material/dialog";
import { InfoPopupDialogComponent } from "./info-popup-dialog/info-popup-dialog.component";
import { MatButtonModule } from "@angular/material/button";



@NgModule({
  declarations: [
    InfoPopupDialogComponent
  ],
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule
  ],
  exports: [
    InfoPopupDialogComponent
  ]
})
export class UtilityComponentsModule { }
