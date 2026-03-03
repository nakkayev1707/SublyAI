# MVP Specification — AI Video Translator & Subtitle Generator

## Project Name
AI Video Translator MVP

## Tech Stack

### Backend
- .NET 8 — ASP.NET Core Web API
- Azure OpenAI or OpenAI Whisper API
- FFmpeg for audio extraction
- Local storage or Azure Blob Storage
- Serilog
- Swagger

### Frontend
- Angular 17
- Bootstrap or Angular Material
- Simple components (file upload, text viewer, video player)

---

# 1. Core Features

## 1.1 Video Upload
User uploads a video file.  
Backend saves it and returns `videoId`.

**Endpoint:**
```
POST /api/videos/upload
multipart/form-data:
  file: video file
Response:
  { videoId: string }
```

---

## 1.2 Transcription (Speech-to-Text)
Backend extracts audio using FFmpeg and sends it to Whisper/OpenAI.

**Endpoint:**
```
POST /api/videos/{videoId}/transcribe
Response:
  { text: string, segments: [ { start, end, text } ] }
```

---

## 1.3 Translation
Backend sends transcription to GPT/translation model.

**Endpoint:**
```
POST /api/videos/{videoId}/translate
Body:
  { targetLanguage: string }
Response:
  { translatedText: string }
```

---

## 1.4 Subtitle Generation (SRT)
Backend generates an `.srt` subtitle file using timestamps from Whisper.

**Endpoint:**
```
POST /api/videos/{videoId}/subtitles
Body:
  { translatedText: string }
Response:
  { subtitleUrl: string }
```

---

# 2. Frontend Requirements (Angular)

## Pages

### 1. Upload Page
- Video file input
- Upload progress bar
- After upload → navigate to Processing page

### 2. Processing Page
- Show transcription text
- Language selector (dropdown)
- Buttons:
  - “Translate”
  - “Generate subtitles”

### 3. Result Page
- Video player
- Button “Load subtitles”
- Button “Download SRT”

---

# 3. Backend Structure

```
/src
  /AITranslator.Api
    Controllers/
      VideosController.cs
    Services/
      ITranscriptionService.cs
      ITranslationService.cs
      ISubtitleService.cs
      TranscriptionService.cs
      TranslationService.cs
      SubtitleService.cs
    Models/
      TranscriptionResult.cs
      TranslationRequest.cs
    Storage/
      FileStorageService.cs
    appsettings.json
```

### Service Logic

#### 1. TranscriptionService
- Extract audio with FFmpeg
- Send audio to Whisper/OpenAI
- Return text + timestamps

#### 2. TranslationService
- Call GPT/Azure OpenAI
- Return translated text

#### 3. SubtitleService
- Build SRT file structure
- Save file
- Return URL

---

# 4. Angular Structure

```
/client
   /src/app
      upload/
        upload.component.ts
      processing/
        processing.component.ts
      result/
        result.component.ts
      services/
        video-api.service.ts
```

### video-api.service.ts methods
- uploadVideo(file)
- transcribe(videoId)
- translate(videoId, targetLanguage)
- generateSubtitles(videoId, text)

---

# 5. Security
- CORS enabled
- Upload size limits
- Validate file type
- Validate target language

---

# 6. Future Enhancements (Not for MVP)
- Voice-over generation
- Embedded subtitles into video rendering
- User authentication and history
- Video trimming before translation

---

# Cursor Instructions
- Generate both backend and frontend scaffolding.
- Create all controllers, DTOs, services, and interfaces.
- Implement Angular components + routing.
- UI can be minimal — functionality first.
