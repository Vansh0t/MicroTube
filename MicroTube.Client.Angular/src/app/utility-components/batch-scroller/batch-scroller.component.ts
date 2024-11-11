import { Component, EventEmitter, Input, Output } from "@angular/core";
import { getScrollTopPercent } from "../../services/utils";

@Component({
  selector: "batch-scroller",
  templateUrl: "./batch-scroller.component.html",
  styleUrl: "./batch-scroller.component.scss"
})
export class BatchScrollerComponent
{
  @Input() isLoading: boolean = false;
  @Input() endOfDataReached: boolean = false;
  @Input() scrollPercentForNewBatch = 0.05;
  @Input() spinnerDiameter = 28;
  @Output() onNextRequiredBatch: EventEmitter<number> = new EventEmitter<number>();

  private prevScrollPercent: number = 0;
  onScroll($event: Event)
  {
    if (!($event.target instanceof (HTMLElement)))
    {
      return;
    }
    const scrollTopPercent = getScrollTopPercent($event.target);
    if (this.isLoading || this.endOfDataReached)
    {
      this.prevScrollPercent = scrollTopPercent;
      return;

    }
    const scrollDelta = scrollTopPercent - this.prevScrollPercent;
    if (scrollDelta > 0 && scrollTopPercent > 1 - this.scrollPercentForNewBatch)
    {
      this.onNextRequiredBatch.emit(scrollDelta);
    }
    this.prevScrollPercent = scrollTopPercent;
  }
}
