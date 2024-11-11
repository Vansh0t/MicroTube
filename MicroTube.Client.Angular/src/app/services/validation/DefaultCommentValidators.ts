import { Injectable } from "@angular/core";
import { AbstractControl, ValidationErrors, Validators } from "@angular/forms";
@Injectable({
  providedIn: "root"
})
export class DefaultCommentValidators
{
  readonly MIN_COMMENT_LENGTH = 1;
  readonly MAX_COMMENT_LENGTH = 512;

  buildCommentContentValidatorsArray(): Array<(control: AbstractControl) => ValidationErrors | null>
  {
    const required = Validators.required;
    const minLength = Validators.minLength(this.MIN_COMMENT_LENGTH);
    const maxLength = Validators.maxLength(this.MAX_COMMENT_LENGTH);
    return [required, minLength, maxLength];
  }
}
