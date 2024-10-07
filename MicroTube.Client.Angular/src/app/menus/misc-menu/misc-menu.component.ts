import { Component } from "@angular/core";
import { MatDialog } from "@angular/material/dialog";
import { InfoPopupDialogComponent } from "../../utility-components/info-popup-dialog/info-popup-dialog.component";
import { InfoService } from "../../services/info/InfoService";

@Component({
  selector: "misc-menu",
  templateUrl: "./misc-menu.component.html",
  styleUrl: "./misc-menu.component.css"
})
export class MiscMenuComponent
{
  readonly infoService: InfoService;
  constructor(infoService: InfoService)
  {
    this.infoService = infoService;
  }
}
