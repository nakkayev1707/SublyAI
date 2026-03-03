import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  VideoUploadResponse,
  TranscriptionResult,
  TranslationResponse,
  SubtitleResponse,
} from '../models/video-api.models';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class VideoApiService {
  private readonly apiBase = environment.apiBaseUrl;

  constructor(private http: HttpClient) {}

  uploadVideo(file: File): Observable<VideoUploadResponse> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<VideoUploadResponse>(`${this.apiBase}/videos/upload`, formData);
  }

  transcribe(videoId: string): Observable<TranscriptionResult> {
    return this.http.post<TranscriptionResult>(
      `${this.apiBase}/videos/${videoId}/transcribe`,
      {}
    );
  }

  translate(videoId: string, targetLanguage: string): Observable<TranslationResponse> {
    return this.http.post<TranslationResponse>(
      `${this.apiBase}/videos/${videoId}/translate`,
      { targetLanguage }
    );
  }

  generateSubtitles(videoId: string, translatedText: string): Observable<SubtitleResponse> {
    return this.http.post<SubtitleResponse>(
      `${this.apiBase}/videos/${videoId}/subtitles`,
      { translatedText }
    );
  }

  getSubtitleDownloadUrl(videoId: string): string {
    return `${this.apiBase}/videos/${videoId}/subtitles/download`;
  }

  getVideoUrl(videoId: string): string {
    return `${this.apiBase}/videos/${videoId}/video`;
  }
}
