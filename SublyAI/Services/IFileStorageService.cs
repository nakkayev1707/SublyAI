namespace SublyAI.Services;

public interface IFileStorageService
{
    Task<string> SaveVideoAsync(IFormFile file, string videoId);
    Task<string> SaveAudioAsync(string videoPath, string videoId);
    Task<string> SaveSubtitleAsync(string content, string videoId);
    Task<string?> GetVideoPathAsync(string videoId);
    Task<string?> GetSubtitlePathAsync(string videoId);
}

