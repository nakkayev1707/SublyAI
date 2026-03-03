using SublyAI.Models;

namespace SublyAI.Services;

public interface ISubtitleService
{
    Task<string> GenerateSubtitleAsync(TranscriptionResult transcription, string translatedText);
}

