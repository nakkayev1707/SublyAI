import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { VideoApiService } from '../services/video-api.service';

@Component({
  selector: 'app-upload',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './upload.component.html',
  styleUrl: './upload.component.css',
})
export class UploadComponent {
  selectedFile: File | null = null;
  uploading = false;
  uploadProgress = 0;
  error = '';

  constructor(
    private videoApi: VideoApiService,
    private router: Router
  ) {}

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (file) {
      const allowed = ['.mp4', '.avi', '.mov', '.mkv', '.webm'];
      const ext = file.name.toLowerCase().slice(file.name.lastIndexOf('.'));
      if (!allowed.includes(ext)) {
        this.error = `Invalid file type. Allowed: ${allowed.join(', ')}`;
        this.selectedFile = null;
        return;
      }
      this.error = '';
      this.selectedFile = file;
    }
  }

  upload(): void {
    if (!this.selectedFile) {
      this.error = 'Please select a video file';
      return;
    }
    this.uploading = true;
    this.error = '';
    this.uploadProgress = 0;

    this.videoApi.uploadVideo(this.selectedFile).subscribe({
      next: (res) => {
        this.uploadProgress = 100;
        this.uploading = false;
        this.router.navigate(['/processing', res.videoId]);
      },
      error: (err) => {
        this.uploading = false;
        this.error = err.error?.message || err.message || 'Upload failed';
      },
    });
  }
}
