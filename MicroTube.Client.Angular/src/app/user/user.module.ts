import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { MatButtonModule } from "@angular/material/button";
import { MatInputModule } from "@angular/material/input";
import { EmailPasswordProfileComponent } from "./email-password-profile/email-password-profile.component";
import { MatCardModule } from "@angular/material/card";
import { MatFormFieldModule } from "@angular/material/form-field";
import { ReactiveFormsModule } from "@angular/forms";
import { AppAuthModule } from "../auth/auth.module";
import { UtilityComponentsModule } from "../utility-components/utility-components.module";
import { ProfilePopupComponent } from "./profile-popup/profile-popup.component";
import { RouterModule } from "@angular/router";
import { MatMenuModule } from "@angular/material/menu";
import { MatIconModule } from "@angular/material/icon";


@NgModule({
  declarations: [
    EmailPasswordProfileComponent,
    ProfilePopupComponent
  ],
  imports: [
    CommonModule,
    MatButtonModule,
    MatInputModule,
    MatCardModule,
    MatFormFieldModule,
    ReactiveFormsModule,
    AppAuthModule,
    UtilityComponentsModule,
    RouterModule,
    MatMenuModule,
    MatIconModule
  ],
  exports: [
    EmailPasswordProfileComponent,
    ProfilePopupComponent
  ]
})
export class UserModule { }
