import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { VideoApiService } from '../services/video-api.service';
import { TranscriptionResult } from '../models/video-api.models';

@Component({
  selector: 'app-processing',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './processing.component.html',
  styleUrl: './processing.component.css',
})
export class ProcessingComponent implements OnInit {
  videoId = '';
  transcription: TranscriptionResult | null = null;
  translatedText = '';
  targetLanguage = 'Spanish';
  loadingTranscribe = false;
  loadingTranslate = false;
  loadingSubtitles = false;
  error = '';
  subtitleGenerated = false;

  readonly languages = [
    'Spanish',
    'French',
    'German',
    'Italian',
    'Portuguese',
    'Japanese',
    'Korean',
    'Chinese',
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private videoApi: VideoApiService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('videoId');
    if (!id) {
      this.router.navigate(['/']);
      return;
    }
    this.videoId = id;
  }

  transcribe(): void {
    this.loadingTranscribe = true;
    this.error = '';
    this.videoApi.transcribe(this.videoId).subscribe({
      next: (res) => {
        this.transcription = res;
        this.loadingTranscribe = false;
      },
      error: (err) => {
        this.loadingTranscribe = false;
        this.error = err.error || err.message || 'Transcription failed';
      },
    });
  }

  translate(): void {
    if (!this.targetLanguage) {
      this.error = 'Select a target language';
      return;
    }
    this.loadingTranslate = true;
    this.error = '';
    this.videoApi.translate(this.videoId, this.targetLanguage).subscribe({
      next: (res) => {
        this.translatedText = res.translatedText;
        this.loadingTranslate = false;
      },
      error: (err) => {
        this.loadingTranslate = false;
        this.error = err.error || err.message || 'Translation failed';
      },
    });
  }

  generateSubtitles(): void {
    const text = this.translatedText?.trim();
    if (!text) {
      this.error = 'Translate first or paste translated text';
      return;
    }
    this.loadingSubtitles = true;
    this.error = '';
    this.videoApi.generateSubtitles(this.videoId, text).subscribe({
      next: () => {
        this.loadingSubtitles = false;
        this.subtitleGenerated = true;
      },
      error: (err) => {
        this.loadingSubtitles = false;
        this.error = err.error || err.message || 'Subtitle generation failed';
      },
    });
  }

  goToResult(): void {
    this.router.navigate(['/result', this.videoId]);
  }
}
