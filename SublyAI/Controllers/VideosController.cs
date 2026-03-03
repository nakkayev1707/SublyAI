using Microsoft.AspNetCore.Mvc;
using SublyAI.Models;
using SublyAI.Services;

namespace SublyAI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VideosController : ControllerBase
{
    private readonly IFileStorageService _fileStorageService;
    private readonly ITranscriptionService _transcriptionService;
    private readonly ITranslationService _translationService;
    private readonly ISubtitleService _subtitleService;
    private readonly ILogger<VideosController> _logger;

    public VideosController(
        IFileStorageService fileStorageService,
        ITranscriptionService transcriptionService,
        ITranslationService translationService,
        ISubtitleService subtitleService,
        ILogger<VideosController> logger)
    {
        _fileStorageService = fileStorageService;
        _transcriptionService = transcriptionService;
        _translationService = translationService;
        _subtitleService = subtitleService;
        _logger = logger;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(500_000_000)] // 500 MB limit
    public async Task<ActionResult<VideoUploadResponse>> UploadVideo(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded");
        }

        // Validate file type
        var allowedExtensions = new[] { ".mp4", ".avi", ".mov", ".mkv", ".webm" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest($"Invalid file type. Allowed types: {string.Join(", ", allowedExtensions)}");
        }

        try
        {
            var videoId = Guid.NewGuid().ToString();
            await _fileStorageService.SaveVideoAsync(file, videoId);

            _logger.LogInformation("Video uploaded successfully. VideoId: {VideoId}", videoId);
            return Ok(new VideoUploadResponse { VideoId = videoId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading video");
            return StatusCode(500, "Error uploading video");
        }
    }

    [HttpPost("{videoId}/transcribe")]
    public async Task<ActionResult<TranscriptionResult>> Transcribe(string videoId)
    {
        try
        {
            var videoPath = await _fileStorageService.GetVideoPathAsync(videoId);
            if (videoPath == null)
            {
                return NotFound($"Video with ID {videoId} not found");
            }

            _logger.LogInformation("Starting transcription for video: {VideoId}", videoId);
            
            // Extract audio
            var audioPath = await _fileStorageService.SaveAudioAsync(videoPath, videoId);
            
            // Transcribe
            var result = await _transcriptionService.TranscribeAsync(audioPath);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transcribing video {VideoId}", videoId);
            return StatusCode(500, $"Error transcribing video: {ex.Message}");
        }
    }

    [HttpPost("{videoId}/translate")]
    public async Task<ActionResult<TranslationResponse>> Translate(string videoId, [FromBody] TranslationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.TargetLanguage))
        {
            return BadRequest("Target language is required");
        }

        try
        {
            // First, we need to get the transcription
            // In a real scenario, you might want to store transcriptions in a database
            // For MVP, we'll require the user to transcribe first
            // This is a simplified approach - in production, you'd store transcriptions
            
            var videoPath = await _fileStorageService.GetVideoPathAsync(videoId);
            if (videoPath == null)
            {
                return NotFound($"Video with ID {videoId} not found");
            }

            // For MVP, we'll transcribe on-the-fly if needed
            // In production, you'd retrieve stored transcription
            var audioPath = Path.Combine(Path.GetDirectoryName(videoPath)!, "audio.wav");
            if (!System.IO.File.Exists(audioPath))
            {
                audioPath = await _fileStorageService.SaveAudioAsync(videoPath, videoId);
            }

            var transcription = await _transcriptionService.TranscribeAsync(audioPath);
            var translatedText = await _translationService.TranslateAsync(transcription.Text, request.TargetLanguage);

            return Ok(new TranslationResponse { TranslatedText = translatedText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error translating video {VideoId}", videoId);
            return StatusCode(500, $"Error translating video: {ex.Message}");
        }
    }

    [HttpPost("{videoId}/subtitles")]
    public async Task<ActionResult<SubtitleResponse>> GenerateSubtitles(string videoId, [FromBody] SubtitleRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.TranslatedText))
        {
            return BadRequest("Translated text is required");
        }

        try
        {
            var videoPath = await _fileStorageService.GetVideoPathAsync(videoId);
            if (videoPath == null)
            {
                return NotFound($"Video with ID {videoId} not found");
            }

            // Get transcription for segments/timestamps
            var audioPath = Path.Combine(Path.GetDirectoryName(videoPath)!, "audio.wav");
            if (!System.IO.File.Exists(audioPath))
            {
                audioPath = await _fileStorageService.SaveAudioAsync(videoPath, videoId);
            }

            var transcription = await _transcriptionService.TranscribeAsync(audioPath);
            
            // Generate SRT
            var srtContent = await _subtitleService.GenerateSubtitleAsync(transcription, request.TranslatedText);
            
            // Save subtitle file
            var subtitlePath = await _fileStorageService.SaveSubtitleAsync(srtContent, videoId);

            // Return URL (in production, this would be a proper URL)
            var subtitleUrl = $"/api/videos/{videoId}/subtitles/download";

            return Ok(new SubtitleResponse { SubtitleUrl = subtitleUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating subtitles for video {VideoId}", videoId);
            return StatusCode(500, $"Error generating subtitles: {ex.Message}");
        }
    }

    [HttpGet("{videoId}/video")]
    public async Task<IActionResult> GetVideo(string videoId)
    {
        try
        {
            var videoPath = await _fileStorageService.GetVideoPathAsync(videoId);
            if (videoPath == null)
            {
                return NotFound($"Video with ID {videoId} not found");
            }

            var contentType = Path.GetExtension(videoPath).ToLowerInvariant() switch
            {
                ".mp4" => "video/mp4",
                ".webm" => "video/webm",
                ".mov" => "video/quicktime",
                ".avi" => "video/x-msvideo",
                ".mkv" => "video/x-matroska",
                _ => "application/octet-stream"
            };
            return PhysicalFile(videoPath, contentType, enableRangeProcessing: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error streaming video {VideoId}", videoId);
            return StatusCode(500, "Error streaming video");
        }
    }

    [HttpGet("{videoId}/subtitles/download")]
    public async Task<IActionResult> DownloadSubtitles(string videoId)
    {
        try
        {
            var subtitlePath = await _fileStorageService.GetSubtitlePathAsync(videoId);
            if (subtitlePath == null)
            {
                return NotFound($"Subtitle file for video {videoId} not found");
            }

            var content = await System.IO.File.ReadAllTextAsync(subtitlePath);
            return File(System.Text.Encoding.UTF8.GetBytes(content), "text/plain", "subtitles.srt");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading subtitles for video {VideoId}", videoId);
            return StatusCode(500, "Error downloading subtitles");
        }
    }
}

