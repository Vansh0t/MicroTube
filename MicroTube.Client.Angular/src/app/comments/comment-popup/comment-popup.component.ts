import { Component, Inject, OnDestroy, ViewChild } from "@angular/core";
import { DIALOG_DATA } from "@angular/cdk/dialog";
import { CdkTextareaAutosize } from "@angular/cdk/text-field";
import { FormControl } from "@angular/forms";
import { DefaultCommentValidators } from "../../services/validation/DefaultCommentValidators";
import { Subscription } from "rxjs";
import { CommentDto } from "../../data/Dto/CommentDto";
import { HttpErrorResponse } from "@angular/common/http";
import { MatDialogRef } from "@angular/material/dialog";
import { CommentingService } from "../../services/comments/CommentingService";

@Component({
  selector: "comment-popup",
  templateUrl: "./comment-popup.component.html",
  styleUrl: "./comment-popup.component.css"
})
export class CommentPopupComponent implements OnDestroy
{
  dialogRef: MatDialogRef<CommentPopupComponent>;
  data: CommentPopupData;
  @ViewChild("autosize") autosize!: CdkTextareaAutosize;
  commentControl: FormControl;
  serverError: string | null = null;

  readonly validators: DefaultCommentValidators;
  private submitSubscription: Subscription | null = null;
  private readonly commentingService: CommentingService;
  constructor(dialogRef: MatDialogRef<CommentPopupComponent>, @Inject(DIALOG_DATA) data: CommentPopupData, validators: DefaultCommentValidators, commentingService: CommentingService)
  {
    if (!data.commentTargetKey || !data.targetId)
    {
      dialogRef.close();
      throw new Error("Invalid data provided. Closing the popup.");
    }
    this.commentingService = commentingService;
    this.validators = validators;
    this.dialogRef = dialogRef;
    this.data = data;
    this.commentControl = new FormControl("", validators.buildCommentContentValidatorsArray());
  }
  ngOnDestroy(): void
  {
    this.submitSubscription?.unsubscribe();
    this.submitSubscription = null;
  }
  getError(): string | null
  {
    if (this.commentControl.hasError("serverError"))
    {
      return this.commentControl.getError("serverError");
    }
    if (this.commentControl.hasError("required"))
    {
      return "Comment must not be empty.";
    }
    if (this.commentControl.hasError("maxLength"))
    {
      return `Comment max length: ${this.validators.MAX_COMMENT_LENGTH}`;
    }
    if (this.commentControl.hasError("minLength"))
    {
      return `Comment max length: ${this.validators.MIN_COMMENT_LENGTH}`;
    }
    return null;
  }
  submit()
  {
    if (this.commentControl.errors)
    {
      return;
    }
    this.serverError = null;
    this.submitSubscription = this.commentingService.comment(this.data.commentTargetKey, this.data.targetId, { content: this.commentControl.value })
      .subscribe(
        {
          next: this.onCommentResponse.bind(this),
          error: (errorResponse: HttpErrorResponse) =>
          {
            console.error(errorResponse);
            this.commentControl.setErrors({ serverError: errorResponse.error });
          }
        });
  }
  cancel()
  {
    this.dialogRef.close(null);
  }
  private onCommentResponse(comment: CommentDto)
  {
    console.log(comment);
    this.dialogRef.close(comment);
  }

}
export interface CommentPopupData
{
  targetId: string;
  userAlias: string | null;
  commentTargetKey: string;
}
