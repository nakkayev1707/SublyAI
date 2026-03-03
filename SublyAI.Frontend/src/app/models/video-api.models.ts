export interface VideoUploadResponse {
  videoId: string;
}

export interface TranscriptionSegment {
  start: number;
  end: number;
  text: string;
}

export interface TranscriptionResult {
  text: string;
  segments: TranscriptionSegment[];
}

export interface TranslationRequest {
  targetLanguage: string;
}

export interface TranslationResponse {
  translatedText: string;
}

export interface SubtitleRequest {
  translatedText: string;
}

export interface SubtitleResponse {
  subtitleUrl: string;
}
