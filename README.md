# SublyAI - AI Video Translator & Subtitle Generator

## Backend Setup

### Prerequisites
- .NET 8 SDK
- FFmpeg (must be installed and available in PATH)
- OpenAI API Key

### Installation

1. **Install FFmpeg**
   - Windows: Download from https://ffmpeg.org/download.html and add to PATH
   - Or use: `winget install ffmpeg`
   - Verify installation: `ffmpeg -version`

2. **Configure OpenAI API Key**
   - Open `SublyAI/appsettings.json`
   - Add your OpenAI API key:
   ```json
   "OpenAI": {
     "ApiKey": "your-api-key-here",
     "Model": "gpt-4"
   }
   ```

3. **Restore NuGet Packages**
   ```bash
   dotnet restore
   ```

4. **Run the Application**
   ```bash
   dotnet run --project SublyAI
   ```

5. **Access Swagger UI**
   - Navigate to: `http://localhost:5000` or `https://localhost:5001`
   - Swagger UI will be available at the root URL

### API Endpoints

- `POST /api/videos/upload` - Upload a video file
- `POST /api/videos/{videoId}/transcribe` - Transcribe video audio
- `POST /api/videos/{videoId}/translate` - Translate transcription
- `POST /api/videos/{videoId}/subtitles` - Generate SRT subtitle file
- `GET /api/videos/{videoId}/subtitles/download` - Download subtitle file

### Storage

Videos and generated files are stored locally in the `Storage` directory (configurable in `appsettings.json`).

### Logging

Logs are written to:
- Console
- File: `logs/sublyai-YYYYMMDD.txt`

### CORS

CORS is configured to allow requests from `http://localhost:4200` (Angular default). Update `appsettings.json` to add more origins.

