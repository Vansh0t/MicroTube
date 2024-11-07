import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { MatFormFieldModule } from "@angular/material/form-field";
import { CommentPopupComponent } from "./comment-popup/comment-popup.component";
import { MatInputModule } from "@angular/material/input";
import { MatButtonModule } from "@angular/material/button";
import { ReactiveFormsModule } from "@angular/forms";
import { CommentsAreaComponent } from "./comments-area/comments-area.component";
import { UtilityComponentsModule } from "../utility-components/utility-components.module";
import { CommentComponent } from "./comment/comment.component";
import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";



@NgModule({
  declarations: [
    CommentPopupComponent,
    CommentsAreaComponent,
    CommentComponent
  ],
  imports: [
    CommonModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    ReactiveFormsModule,
    UtilityComponentsModule,
    MatProgressSpinnerModule
  ],
  exports: [
    CommentPopupComponent,
    CommentsAreaComponent
  ]
})
export class CommentsModule { }
