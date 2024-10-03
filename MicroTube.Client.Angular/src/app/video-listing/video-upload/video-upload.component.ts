/* eslint-disable quotes */
import { Component, OnDestroy, ViewChild } from "@angular/core";
import { FormControl, FormGroup, Validators } from "@angular/forms";
import { FileInput } from "ngx-custom-material-file-input";
import { DefaultVideoFileUploadValidators } from "../../services/validation/DefaultVideoFileUploadValidators";
import { CdkTextareaAutosize } from "@angular/cdk/text-field";
import { VideoService } from "../../services/videos/VideoService";
import { VideoUploadDTO } from "../../data/DTO/VideoDTO";
import { Router } from "@angular/router";
import { HttpErrorResponse, HttpEvent, HttpEventType } from "@angular/common/http";
import { Subscription } from "rxjs";
import { InfoService } from "../../services/info/InfoService";
import { VideoUploadProgressDTO } from "../../data/DTO/VideoUploadProgressDTO";
import { MatProgressSpinner } from "@angular/material/progress-spinner";

@Component({
  selector: "video-upload",
  templateUrl: "./video-upload.component.html",
  styleUrls: ["./video-upload.component.css"]
})
export class VideoUploadComponent implements OnDestroy
{
  private readonly videoService: VideoService;
  private readonly router: Router;
  readonly videoUploadValidation: DefaultVideoFileUploadValidators;

  readonly infoService: InfoService;
  readonly titleControl: FormControl;
  readonly descriptionControl: FormControl;
  readonly policiesControl: FormControl;
  readonly fileControl: FormControl<FileInput | null>;
  readonly formGroup: FormGroup;
  readonly acceptString : string;
  @ViewChild('autosize') autosize!: CdkTextareaAutosize;
  uploadProgressSubscription: Subscription|null = null;
  uploadServerError: string | null = null;
  @ViewChild("uploadSpinner") uploadSpinner!: MatProgressSpinner;
  uploadPercent: number = 0;
  get uploadPercentRounded() { return Math.round(this.uploadPercent); }
  constructor(videoUploadValidation: DefaultVideoFileUploadValidators, videoService: VideoService, router: Router, infoService: InfoService)
  {
    this.infoService = infoService;
    this.videoService = videoService;
    this.videoUploadValidation = videoUploadValidation;
    this.router = router;
    this.titleControl = new FormControl("",
      [Validators.required, Validators.minLength(videoUploadValidation.MIN_TITLE_LENGTH), Validators.maxLength(videoUploadValidation.MAX_TITLE_LENGTH)]);
    this.fileControl = new FormControl<FileInput | null>(null, [Validators.required, videoUploadValidation.getMaxSizeValidator()]);
    this.descriptionControl = new FormControl("", [Validators.maxLength(videoUploadValidation.MAX_DESCRIPTION_LENGTH)]);
    this.policiesControl = new FormControl(false, [Validators.requiredTrue]);
    this.formGroup = new FormGroup([this.titleControl, this.descriptionControl, this.policiesControl]);
    this.formGroup.addControl("fileControl", this.fileControl);
    this.acceptString = videoUploadValidation.getAcceptString(); 
  }
    


  submit()
  {
    if (!this.formGroup.valid || this.fileControl.value == null)
      return;
    const videoData: VideoUploadDTO = {
      title: this.titleControl.value,
      description: this.descriptionControl.value,
      file: this.fileControl.value
    };
    this.uploadProgressSubscription = this.videoService.uploadVideo(videoData)
      .subscribe({
        next: this.onUploadEvent.bind(this),
      error: (error: HttpErrorResponse) =>
      {
        this.uploadProgressSubscription?.unsubscribe();
        this.uploadProgressSubscription = null;
        this.uploadServerError = error.error;
      }
    });
    this.uploadServerError = null;
  }
  getTitleError(): string | null
  {
    let error = "";
    if (this.titleControl.hasError("required"))
      error += "Title is required. ";
    if (this.titleControl.hasError("minLength"))
      error += `Title at least ${this.videoUploadValidation.MIN_TITLE_LENGTH} characters. `;
    if (this.titleControl.hasError("maxLength"))
      error += `Title max length: ${this.videoUploadValidation.MAX_TITLE_LENGTH} characters. `;
    return error != "" ? error : null;
  }
  getDescriptionError(): string | null
  {
    if (this.descriptionControl.hasError("maxLength"))
    {
      return `Description max length: ${this.videoUploadValidation.MAX_DESCRIPTION_LENGTH}`;
    }
    return null;
  }
  getPoliciesError(): string | null
  {
    if (this.policiesControl.hasError("requiredTrue"))
    {
      return "Please, accept the policies to continue.";
    }
    return null;
  }
  getFileError(): string | null
  {
    if (this.fileControl.hasError("required"))
      return "Video File is required";
    if (this.fileControl.hasError("maxContentSize"))
      return `File is too big. Max size: ${this.videoUploadValidation.MAX_FILE_SIZE_BYTES} bytes`;
    return null;
  }
  ngOnDestroy(): void
  {
    if (this.uploadProgressSubscription != null)
      this.uploadProgressSubscription.unsubscribe();
  }
  private onUploadEvent(event: HttpEvent<VideoUploadProgressDTO>)
  {
    if (event.type === HttpEventType.UploadProgress && event.total)
    {
      this.uploadPercent = (event.loaded / event.total) * 100;
      console.log(this.uploadPercent);
    }
    if (event.type === HttpEventType.Response)
    {
      this.router.navigate(["upload/list"]);
    }
  }
}
