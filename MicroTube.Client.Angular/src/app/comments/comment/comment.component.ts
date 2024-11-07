import { Component, Input, OnInit } from "@angular/core";
import { CommentDTO } from "../../data/DTO/CommentDTO";
import { TimeFormatter } from "../../services/formatting/TimeFormatter";
import { DateTime } from "luxon";

@Component({
  selector: "comment",
  templateUrl: "./comment.component.html",
  styleUrl: "./comment.component.css"
})
export class CommentComponent implements OnInit
{
  @Input() comment: CommentDTO | undefined;

  private readonly timeFormatter: TimeFormatter;
  constructor(timeFormatter: TimeFormatter)
  {
    this.timeFormatter = timeFormatter;
  }
  ngOnInit(): void {
    if (!this.comment)
    {
      throw new Error("comment is required");
    }
  }
  getFormattedTime()
  {
    if (!this.comment)
    {
      return "";
    }
    const uploadTimeLocal = this.comment.time.toLocal();
    const nowLocal = DateTime.local();
    return this.timeFormatter.getUserFriendlyTimeDifference(uploadTimeLocal, nowLocal);
  }
}
