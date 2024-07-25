import { Component, OnDestroy, OnInit, ViewChild } from "@angular/core";
import { AuthManager } from "../../services/auth/AuthManager";
import { VideoService } from "../../services/videos/VideoService";
import { Subscription } from "rxjs";
import { VideoUploadProgressDTO } from "../../data/DTO/VideoUploadProgressDTO";
import { MatTableDataSource } from "@angular/material/table";
import { MatPaginator } from "@angular/material/paginator";
import { MatSort } from "@angular/material/sort";
import { DateTime } from "luxon";

@Component({
  selector: "upload-progress-list",
  templateUrl: "./upload-progress-list.component.html",
  styleUrls: ["./upload-progress-list.component.css"]
})
export class UploadProgressListComponent implements OnInit, OnDestroy
{
  private readonly videoService: VideoService;
  readonly authManager: AuthManager;

  @ViewChild(MatPaginator) paginator = {} as MatPaginator;
  @ViewChild(MatSort) sort = {} as MatSort;

  progressListSubscription: Subscription | null = null;

  uiListDataSource = new MatTableDataSource<VideoUploadProgressDTO>();
  tableColumns: string[] = ["timestamp", "status", "title", "description", "lengthSeconds", "format", "fps"];

  constructor(videoService: VideoService, authManager: AuthManager)
  {
    this.videoService = videoService;
    this.authManager = authManager;
    this.uiListDataSource.sortingDataAccessor = (item, headerId): string | number =>
    {
      if (headerId == "timestamp" && item.timestampHuman != null)
      {
        return DateTime.fromISO(item.timestamp).toUnixInteger();
      }
      const value = item[headerId as keyof VideoUploadProgressDTO];
      return value != null ? value: 0;
    };
  }

  ngOnInit(): void
  {
    if (this.authManager.isSignedIn())
    {
      this.progressListSubscription = this.videoService.getUploadProgressList()
        .subscribe({
          next: this.onProgressListUpdated.bind(this)
        });
    }
  }
  ngOnDestroy(): void {
    if (this.progressListSubscription != null)
    {
      this.progressListSubscription.unsubscribe();
      this.progressListSubscription = null;
    }
  }
  onProgressListUpdated(list: VideoUploadProgressDTO[]): void
  {
    this.uiListDataSource.data = list;
    this.uiListDataSource.paginator = this.paginator;
    this.uiListDataSource.sort = this.sort;
  }
  getProgressStatusStyleClass(uploadProgress: VideoUploadProgressDTO): string
  {
    switch (uploadProgress.status.trim().toLowerCase())
    {
      case "inqueue":
        return "status-in-queue";
      case "inprogress":
        return "status-in-progress";
      case "fail":
        return "status-fail";
      case "success":
        return "status-success";
      default:
        return "";
    }
  }
  getProgressStatusIcon(uploadProgress: VideoUploadProgressDTO): string
  {
    switch (uploadProgress.status.trim().toLowerCase())
    {
      case "inqueue":
        return "schedule";
      case "inprogress":
        return "autorenew";
      case "fail":
        return "close";
      case "success":
        return "done";
      default:
        return "";
    }
  }
}
