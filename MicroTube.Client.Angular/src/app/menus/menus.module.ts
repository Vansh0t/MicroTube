import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { MiscMenuComponent } from "./misc-menu/misc-menu.component";
import { MatIconModule } from "@angular/material/icon";
import { MatMenuModule } from "@angular/material/menu";
import { MatButtonModule } from "@angular/material/button";



@NgModule({
  declarations: [
    MiscMenuComponent
  ],
  imports: [
    CommonModule,
    MatIconModule,
    MatMenuModule,
    MatButtonModule
  ],
  exports: [
    MiscMenuComponent
  ]

})
export class MenusModule { }
