using SublyAI.Models;

namespace SublyAI.Services;

public class SubtitleService : ISubtitleService
{
    private readonly ILogger<SubtitleService> _logger;

    public SubtitleService(ILogger<SubtitleService> logger)
    {
        _logger = logger;
    }

    public Task<string> GenerateSubtitleAsync(TranscriptionResult transcription, string translatedText)
    {
        // Split translated text into sentences/segments
        // For simplicity, we'll use the original segments and map translated text
        // In a more sophisticated implementation, you'd align the translated text with segments
        
        var srtContent = new System.Text.StringBuilder();
        var translatedSentences = SplitIntoSentences(translatedText);
        
        // Use original segments if available, otherwise create time-based segments
        if (transcription.Segments.Any())
        {
            for (int i = 0; i < transcription.Segments.Count; i++)
            {
                var segment = transcription.Segments[i];
                var translatedSegment = i < translatedSentences.Count 
                    ? translatedSentences[i] 
                    : translatedText; // Fallback to full text if not enough sentences

                srtContent.AppendLine($"{i + 1}");
                srtContent.AppendLine($"{FormatTime(segment.Start)} --> {FormatTime(segment.End)}");
                srtContent.AppendLine(translatedSegment);
                srtContent.AppendLine();
            }
        }
        else
        {
            // Fallback: create a single subtitle entry
            srtContent.AppendLine("1");
            srtContent.AppendLine("00:00:00,000 --> 00:00:10,000");
            srtContent.AppendLine(translatedText);
        }

        var result = srtContent.ToString();
        _logger.LogInformation("SRT subtitle generated. Length: {Length}", result.Length);
        
        return Task.FromResult(result);
    }

    private static string FormatTime(double seconds)
    {
        var timeSpan = TimeSpan.FromSeconds(seconds);
        return $"{(int)timeSpan.TotalHours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2},{timeSpan.Milliseconds:D3}";
    }

    private static List<string> SplitIntoSentences(string text)
    {
        // Simple sentence splitting - can be improved
        var sentences = text.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList();
        
        return sentences.Any() ? sentences : new List<string> { text };
    }
}

