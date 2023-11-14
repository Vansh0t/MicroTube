import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { ReactiveFormsModule } from "@angular/forms";
import { SignInFormComponent } from "./sign-in-form/sign-in-form.component";
import { MatCheckboxModule } from "@angular/material/checkbox";
import { MatButtonModule } from "@angular/material/button";
import { SignUpFormComponent } from "./sign-up-form/sign-up-form.component";
import { SignOutFormComponent } from "./sign-out-form/sign-out-form.component";
import { MatIconModule } from "@angular/material/icon";
import { EmailConfirmationCallbackComponent } from "./email-confirmation-callback/email-confirmation-callback.component";
import { MatMenuModule } from "@angular/material/menu";
import { ResetPasswordFormComponent } from "./reset-password-form/reset-password-form.component";
import { UtilityComponentsModule } from "../utility-components/utility-components.module";
import { EmailConfirmationReminderComponent } from "./email-confirmation-reminder/email-confirmation-reminder.component";
import { MatSnackBarModule } from "@angular/material/snack-bar";
import { PasswordChangeFormComponent } from './password-change-form/password-change-form.component';



@NgModule({
  declarations: [
    SignInFormComponent,
    SignUpFormComponent,
    SignOutFormComponent,
    EmailConfirmationCallbackComponent,
    ResetPasswordFormComponent,
    EmailConfirmationReminderComponent,
    PasswordChangeFormComponent
  ],
  imports: [
    CommonModule,
    MatFormFieldModule,
    MatInputModule,
    ReactiveFormsModule,
    MatCheckboxModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    UtilityComponentsModule,
    MatSnackBarModule
  ],
  exports: [
    SignUpFormComponent,
    SignInFormComponent,
    SignOutFormComponent,
    EmailConfirmationCallbackComponent,
    EmailConfirmationReminderComponent
  ]
})
export class AppAuthModule { }
