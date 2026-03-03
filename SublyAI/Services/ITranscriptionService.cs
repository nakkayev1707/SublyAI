using SublyAI.Models;

namespace SublyAI.Services;

public interface ITranscriptionService
{
    Task<TranscriptionResult> TranscribeAsync(string audioPath);
}

