import { Component, Input } from "@angular/core";
import { FormControl } from "@angular/forms";
import { Observable } from "rxjs";

@Component({
  selector: "suggestion-search-bar",
  templateUrl: "./suggestion-search-bar.component.html",
  styleUrls: ["./suggestion-search-bar.component.css"]
})
export class SuggestionSearchBarComponent
{
  private readonly MAX_SUGGESTIONS = 6;

  readonly inputControl = new FormControl<string>("");
  @Input() onSubmit: ((searchText: string | null) => void) | undefined;
  @Input() onInputChanged: ((searchText: string | null) => void) | undefined;
  @Input() suggestionsSource: Observable<string[]> | undefined;

  submit()
  {
    const inputText = this.inputControl.value;
    if (this.onSubmit != null)
      this.onSubmit(inputText);
  }
  inputChanged()
  {
    const inputText = this.inputControl.value;
    if (this.onInputChanged != null)
      this.onInputChanged(inputText);
  }
}
