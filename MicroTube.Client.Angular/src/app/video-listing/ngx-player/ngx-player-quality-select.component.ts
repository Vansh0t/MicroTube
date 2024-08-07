import { Component, EventEmitter, Input, Output } from "@angular/core";
import { QualityOption } from "./ngx-player.component";
import { MatIconModule } from "@angular/material/icon";
@Component({
  standalone: true,
  selector: "ngx-player-quality-selector",
  templateUrl: "./ngx-player-quality-select.component.html",
  styleUrls: ["./ngx-player-quality-select.component.css"],
  imports: [
    MatIconModule
  ]
})
export class NgxPlayerQualitySelectorComponent
{
  @Input() qualityOptions: QualityOption[] | null = null;
  @Input() selectedQualityIndex: number = -1;
  @Output() onQualityChanged = new EventEmitter<QualityOption>();
  private _isExpanded: boolean = false;
  get isExpanded()
  {
    return this._isExpanded;
  }
  set isExpanded(value: boolean)
  {
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
}
