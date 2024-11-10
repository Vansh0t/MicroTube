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
import { BatchScrollerComponent } from "./batch-scroller/batch-scroller.component";
import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";
import { ConfirmPopupDialogComponent } from "./confirm-popup-dialog/confirm-popup-dialog.component";



@NgModule({
  declarations: [
    InfoPopupDialogComponent,
    SuggestionSearchBarComponent,
    BatchScrollerComponent,
    ConfirmPopupDialogComponent
  ],
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatInputModule,
    MatIconModule,
    MatFormFieldModule,
    ReactiveFormsModule,
    MatAutocompleteModule,
    MatProgressSpinnerModule
  ],
  exports: [
    InfoPopupDialogComponent,
    SuggestionSearchBarComponent,
    BatchScrollerComponent,
    ConfirmPopupDialogComponent
  ]
})
export class UtilityComponentsModule { }
