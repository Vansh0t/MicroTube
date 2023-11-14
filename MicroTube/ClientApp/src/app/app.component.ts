import { Component, ViewChild } from "@angular/core";
import { AuthManager } from "./services/auth/AuthManager";
import { MatMenuTrigger } from "@angular/material/menu";

@Component({
  selector: "app-root",
  templateUrl: "./app.component.html"
})
export class AppComponent {
  title = "app";
  readonly authManager: AuthManager;
  @ViewChild(MatMenuTrigger)
  signOutMenuTrigger!: MatMenuTrigger;
  constructor(authManager: AuthManager)
  {
    this.authManager = authManager;
  }
  isUserEmailConfirmed()
  {
    return this.authManager.jwtSignedInUser$.value?.isEmailConfirmed;
  }
  closeSignOutMenu()
  {
    this.signOutMenuTrigger.closeMenu();
  }
}
