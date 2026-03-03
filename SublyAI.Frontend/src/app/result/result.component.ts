import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { VideoApiService } from '../services/video-api.service';

@Component({
  selector: 'app-result',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './result.component.html',
  styleUrl: './result.component.css',
})
export class ResultComponent {
  videoId = '';

  constructor(
    private route: ActivatedRoute,
    private videoApi: VideoApiService
  ) {
    const id = this.route.snapshot.paramMap.get('videoId');
    if (id) this.videoId = id;
  }

  get videoSrc(): string {
    return this.videoApi.getVideoUrl(this.videoId);
  }

  get subtitleTrackSrc(): string {
    return this.videoApi.getSubtitleDownloadUrl(this.videoId);
  }

  get downloadSubtitleUrl(): string {
    return this.videoApi.getSubtitleDownloadUrl(this.videoId);
  }
}
