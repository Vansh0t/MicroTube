import { Injectable } from "@angular/core";
import { ValidatorFn} from "@angular/forms";
import { FileValidator } from "ngx-custom-material-file-input";
@Injectable({
  providedIn: "root"
})
export class DefaultVideoFileUploadValidators
{
  private readonly ACCEPT_TYPES = "video/mp4";
  readonly MAX_FILE_SIZE_BYTES = 3221225472; //3gb
  readonly MIN_TITLE_LENGTH = 2;
  readonly MAX_TITLE_LENGTH = 200;
  readonly MAX_DESCRIPTION_LENGTH = 1000;

  getMaxSizeValidator(): ValidatorFn
  {
    return FileValidator.maxContentSize(this.MAX_FILE_SIZE_BYTES);
  }
  getAcceptString(): string
  {
    return this.ACCEPT_TYPES;
  }
}
