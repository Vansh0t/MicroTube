import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SuggestionSearchBarComponent } from './suggestion-search-bar.component';

describe('SuggestionSearchBarComponent', () => {
  let component: SuggestionSearchBarComponent;
  let fixture: ComponentFixture<SuggestionSearchBarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SuggestionSearchBarComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SuggestionSearchBarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
