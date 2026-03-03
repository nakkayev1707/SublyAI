using System.IO;

namespace SublyAI.Services;

public class FileStorageService : IFileStorageService
{
    private readonly string _storagePath;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(IConfiguration configuration, ILogger<FileStorageService> logger)
    {
        _storagePath = configuration["Storage:LocalPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "Storage");
        _logger = logger;
        
        // Ensure storage directory exists
        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
    }

    public async Task<string> SaveVideoAsync(IFormFile file, string videoId)
    {
        var videoDir = Path.Combine(_storagePath, videoId);
        if (!Directory.Exists(videoDir))
        {
            Directory.CreateDirectory(videoDir);
        }

        var extension = Path.GetExtension(file.FileName);
        var videoPath = Path.Combine(videoDir, $"video{extension}");
        
        using (var stream = new FileStream(videoPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        _logger.LogInformation("Video saved: {VideoPath}", videoPath);
        return videoPath;
    }

    public async Task<string> SaveAudioAsync(string videoPath, string videoId)
    {
        var videoDir = Path.Combine(_storagePath, videoId);
        var audioPath = Path.Combine(videoDir, "audio.wav");

        // FFmpeg command to extract audio
        var ffmpegPath = "ffmpeg"; // Assumes ffmpeg is in PATH
        var arguments = $"-i \"{videoPath}\" -vn -acodec pcm_s16le -ar 16000 -ac 1 \"{audioPath}\"";

        var processInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = ffmpegPath,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = System.Diagnostics.Process.Start(processInfo);
        if (process != null)
        {
            await process.WaitForExitAsync();
            if (process.ExitCode != 0)
            {
                var error = await process.StandardError.ReadToEndAsync();
                _logger.LogError("FFmpeg error: {Error}", error);
                throw new Exception($"Failed to extract audio: {error}");
            }
        }

        _logger.LogInformation("Audio extracted: {AudioPath}", audioPath);
        return audioPath;
    }

    public async Task<string> SaveSubtitleAsync(string content, string videoId)
    {
        var videoDir = Path.Combine(_storagePath, videoId);
        if (!Directory.Exists(videoDir))
        {
            Directory.CreateDirectory(videoDir);
        }

        var subtitlePath = Path.Combine(videoDir, "subtitles.srt");
        await File.WriteAllTextAsync(subtitlePath, content);

        _logger.LogInformation("Subtitle saved: {SubtitlePath}", subtitlePath);
        return subtitlePath;
    }

    public Task<string?> GetVideoPathAsync(string videoId)
    {
        var videoDir = Path.Combine(_storagePath, videoId);
        if (!Directory.Exists(videoDir))
        {
            return Task.FromResult<string?>(null);
        }

        var videoFiles = Directory.GetFiles(videoDir, "video.*");
        var videoPath = videoFiles.FirstOrDefault();
        return Task.FromResult<string?>(videoPath);
    }

    public Task<string?> GetSubtitlePathAsync(string videoId)
    {
        var subtitlePath = Path.Combine(_storagePath, videoId, "subtitles.srt");
        if (!File.Exists(subtitlePath))
        {
            return Task.FromResult<string?>(null);
        }

        return Task.FromResult<string?>(subtitlePath);
    }
}

