import { Component, Input, ViewChild } from "@angular/core";
import { FormControl } from "@angular/forms";
import { MatAutocomplete } from "@angular/material/autocomplete";

@Component({
  selector: "suggestion-search-bar",
  templateUrl: "./suggestion-search-bar.component.html",
  styleUrls: ["./suggestion-search-bar.component.css"]
})
export class SuggestionSearchBarComponent
{
  readonly inputControl = new FormControl<string>("");
  @Input() onSubmit: ((searchText: string | null) => void) | undefined;
  @Input() onInputChanged: ((searchText: string | null) => void) | undefined;
  @Input() suggestionsSource: string[] | null = null;

  @ViewChild("auto") autocomplete!: MatAutocomplete;

  submit()
  {
    const inputText = this.inputControl.value;
    if (this.onSubmit != null)
    {
      this.onSubmit(inputText);
      this.autocomplete.options.forEach(_ => _.deselect());
    }
  }
  inputChanged()
  {
    const inputText = this.inputControl.value;
    if (this.onInputChanged != null)
      this.onInputChanged(inputText);
  }
}
