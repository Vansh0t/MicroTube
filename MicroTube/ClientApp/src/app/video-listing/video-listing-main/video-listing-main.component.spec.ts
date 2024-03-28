import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VideoListingMainComponent } from './video-listing-main.component';

describe('VideoListingMainComponent', () => {
  let component: VideoListingMainComponent;
  let fixture: ComponentFixture<VideoListingMainComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ VideoListingMainComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VideoListingMainComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
