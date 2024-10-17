import { Component, EventEmitter, Input, OnDestroy, Output } from "@angular/core";
import { QualityOption } from "./ngx-player.component";
import { MatIconModule } from "@angular/material/icon";
import { BreakpointObserver, BreakpointState, Breakpoints } from "@angular/cdk/layout";
import { Subscription } from "rxjs";
import { MatDialog, MatDialogRef } from "@angular/material/dialog";
import { VideoPlayerQualityDialogComponent } from "../video-player-quality-dialog/video-player-quality-dialog.component";
@Component({
  standalone: true,
  selector: "ngx-player-quality-selector",
  templateUrl: "./ngx-player-quality-select.component.html",
  styleUrls: ["./ngx-player-quality-select.component.scss"],
  imports: [
    MatIconModule
  ]
})
export class NgxPlayerQualitySelectorComponent implements OnDestroy
{
  @Input() qualityOptions: QualityOption[] | null = null;
  @Input() selectedQualityIndex: number = -1;
  @Output() onQualityChanged = new EventEmitter<QualityOption>();
  usePopupSelect: boolean = false;
  private _isExpanded: boolean = false;
  private readonly breakpointObserver: BreakpointObserver;
  private readonly dialog: MatDialog;
  private breakpointSubscription: Subscription;
  private qualitySelectDialogRef: MatDialogRef<VideoPlayerQualityDialogComponent, QualityOption> | null = null;
  constructor(breakpointObserver: BreakpointObserver, dialog: MatDialog)
  {
    this.dialog = dialog;
    this.breakpointObserver = breakpointObserver;
    this.breakpointSubscription = this.breakpointObserver.observe([Breakpoints.XSmall])
      .subscribe(this.onBreakpoint.bind(this));
  }
  ngOnDestroy(): void
  {
    this.breakpointSubscription.unsubscribe();
  }
  get isExpanded()
  {
    return this._isExpanded;
  }
  set isExpanded(value: boolean)
  {
    if (this.usePopupSelect && value)
    {
      this.showQualitySelectDialog();
      return;
    }
    this._isExpanded = value;
  }
  get selectedQuality()
  {
    if (this.selectedQualityIndex == -1 || this.qualityOptions == null || this.qualityOptions.length == 0)
      return null;
    return this.qualityOptions[this.selectedQualityIndex];
  }
  setQuality(index: number)
  {
    let isChanged = false;
    if (this.selectedQualityIndex != index)
      isChanged = true;
    this.selectedQualityIndex = index;
    if (isChanged && this.selectedQuality)
      this.onQualityChanged.emit(this.selectedQuality);
  }
  private showQualitySelectDialog()
  {
    this.qualitySelectDialogRef = this.dialog.open(VideoPlayerQualityDialogComponent,
      { data: { qualityOptions: this.qualityOptions, selectedQuality: this.selectedQuality } });
    this.qualitySelectDialogRef.afterClosed().subscribe((resultOption) =>
    {
      this.qualitySelectDialogRef = null;
      if (!this.qualityOptions || !resultOption)
      {
        return;
      }
      this.setQuality(this.qualityOptions.indexOf(resultOption));
    });
  }
  private onBreakpoint(state: BreakpointState)
  {
    this.usePopupSelect = state.matches;
    this.qualitySelectDialogRef?.close();
    this.qualitySelectDialogRef = null;
    this.isExpanded = false;
  }
}
