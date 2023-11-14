import { Component, EventEmitter, Output } from "@angular/core";
import { AuthManager } from "../../services/auth/AuthManager";

@Component({
  selector: "sign-out-form",
  templateUrl: "./sign-out-form.component.html",
  styleUrls: ["./sign-out-form.component.css"]
})
export class SignOutFormComponent
{
  @Output() onConfirmed = new EventEmitter();
  @Output() onCancelled = new EventEmitter();

  private readonly authManager: AuthManager;

  constructor(authManager: AuthManager)
  {
    this.authManager = authManager;
  }

  confirm()
  {
    this.authManager.signOut();
    this.onConfirmed.emit();
  }
  cancel()
  {
    this.onCancelled.emit();
  }
}
