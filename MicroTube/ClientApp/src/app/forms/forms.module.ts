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



@NgModule({
  declarations: [
    SignInFormComponent,
    SignUpFormComponent,
    SignOutFormComponent
  ],
  imports: [
    CommonModule,
    MatFormFieldModule,
    MatInputModule,
    ReactiveFormsModule,
    MatCheckboxModule,
    MatButtonModule,
    MatIconModule
  ],
  exports: [
    SignUpFormComponent,
    SignInFormComponent,
    SignOutFormComponent
  ]
})
export class AppFormsModule { }
