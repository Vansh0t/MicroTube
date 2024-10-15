import { Component } from "@angular/core";
import { InfoService } from "../../services/info/InfoService";
import { ThemeManager } from "../../services/ui/ThemeManager";

@Component({
  selector: "misc-menu",
  templateUrl: "./misc-menu.component.html",
  styleUrl: "./misc-menu.component.css"
})
export class MiscMenuComponent
{
  readonly themeManager: ThemeManager;
  readonly infoService: InfoService;
  get isDarkTheme()
  {
    return this.themeManager.theme.value === "dark-theme";
  }
  constructor(infoService: InfoService, themeManager: ThemeManager)
  {
    this.infoService = infoService;
    this.themeManager = themeManager;
  }
}
