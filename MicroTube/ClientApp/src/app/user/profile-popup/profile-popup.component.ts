import { Component, ViewChild } from "@angular/core";
import { AuthManager } from "../../services/auth/AuthManager";
import { MatMenuTrigger } from "@angular/material/menu";

@Component({
  selector: "profile-popup",
  templateUrl: "./profile-popup.component.html",
  styleUrls: ["./profile-popup.component.css"]
})
export class ProfilePopupComponent
{
  readonly authManager: AuthManager;
  @ViewChild(MatMenuTrigger)
  signOutMenuTrigger!: MatMenuTrigger;
  constructor(authManager: AuthManager)
  {
    this.authManager = authManager;
  }
  closeSignOutMenu()
  {
    this.signOutMenuTrigger.closeMenu();
  }
}
