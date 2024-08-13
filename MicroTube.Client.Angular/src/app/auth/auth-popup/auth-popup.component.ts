import { Component, OnDestroy } from "@angular/core";
import { MatDialogRef } from "@angular/material/dialog";
import { AuthManager } from "../../services/auth/AuthManager";
import { Subscription } from "rxjs";
import { JWTUser } from "../../services/auth/JWTUser";

@Component({
  selector: "auth-popup",
  templateUrl: "./auth-popup.component.html",
  styleUrl: "./auth-popup.component.scss"
})
export class AuthPopupComponent implements OnDestroy
{
  readonly dialogRef: MatDialogRef<AuthPopupComponent>;
  private authStateChangedSubscription: Subscription;
  constructor(dialogRef: MatDialogRef<AuthPopupComponent>, auth: AuthManager)
  {
    this.dialogRef = dialogRef;
    this.authStateChangedSubscription = auth.jwtSignedInUser$.subscribe(this.closeIfSignedIn.bind(this));
    this.closeIfSignedIn(auth.jwtSignedInUser$.value);
  }
  ngOnDestroy(): void
  {
    this.authStateChangedSubscription.unsubscribe();
  }
  close()
  {
    this.dialogRef.close();
  }
  private closeIfSignedIn(user: JWTUser | null)
  {
    if (user)
    {
      this.close();
    }
  }
}
