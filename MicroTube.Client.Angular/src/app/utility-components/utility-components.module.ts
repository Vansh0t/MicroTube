import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { MatDialogModule } from "@angular/material/dialog";
import { InfoPopupDialogComponent } from "./info-popup-dialog/info-popup-dialog.component";
import { MatButtonModule } from "@angular/material/button";
import { SuggestionSearchBarComponent } from "./suggestion-search-bar/suggestion-search-bar.component";
import { MatInputModule } from "@angular/material/input";
import { MatIconModule } from "@angular/material/icon";
import { MatFormFieldModule } from "@angular/material/form-field";
import { ReactiveFormsModule } from "@angular/forms";
import { MatAutocompleteModule } from "@angular/material/autocomplete";



@NgModule({
  declarations: [
    InfoPopupDialogComponent,
    SuggestionSearchBarComponent
  ],
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatInputModule,
    MatIconModule,
    MatFormFieldModule,
    ReactiveFormsModule,
    MatAutocompleteModule
  ],
  exports: [
    InfoPopupDialogComponent,
    SuggestionSearchBarComponent
  ]
})
export class UtilityComponentsModule { }
