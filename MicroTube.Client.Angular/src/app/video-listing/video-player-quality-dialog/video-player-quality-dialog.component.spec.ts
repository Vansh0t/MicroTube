import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VideoPlayerQualityDialogComponent } from './video-player-quality-dialog.component';

describe('VideoPlayerQualityDialogComponent', () => {
  let component: VideoPlayerQualityDialogComponent;
  let fixture: ComponentFixture<VideoPlayerQualityDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [VideoPlayerQualityDialogComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(VideoPlayerQualityDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
