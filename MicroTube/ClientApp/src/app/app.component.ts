import { Component, ViewChild } from "@angular/core";
import { AuthManager } from "./services/auth/AuthManager";
import { MatMenuTrigger } from "@angular/material/menu";
import { SessionManager } from "./services/auth/SessionManager";

@Component({
  selector: "app-root",
  templateUrl: "./app.component.html",
  styleUrls: ["./app.component.scss"]
})
export class AppComponent {
  title = "app";
  private readonly sessionManager: SessionManager;
  readonly authManager: AuthManager;
  @ViewChild(MatMenuTrigger)
  signOutMenuTrigger!: MatMenuTrigger;
  constructor(authManager: AuthManager, sessionManager: SessionManager)
  {
    this.authManager = authManager;
    this.sessionManager = sessionManager;
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
