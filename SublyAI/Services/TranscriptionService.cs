using SublyAI.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SublyAI.Services;

public class TranscriptionService : ITranscriptionService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TranscriptionService> _logger;
    private readonly HttpClient _httpClient;

    public TranscriptionService(IConfiguration configuration, ILogger<TranscriptionService> logger, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<TranscriptionResult> TranscribeAsync(string audioPath)
    {
        var apiKey = _configuration["OpenAI:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new Exception("OpenAI API key is not configured");
        }

        _logger.LogInformation("Sending audio to Whisper API for transcription");

        using var content = new MultipartFormDataContent();
        var audioBytes = await File.ReadAllBytesAsync(audioPath);
        var audioContent = new ByteArrayContent(audioBytes);
        audioContent.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");
        content.Add(audioContent, "file", Path.GetFileName(audioPath));
        content.Add(new StringContent("whisper-1"), "model");
        content.Add(new StringContent("verbose_json"), "response_format");

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/audio/transcriptions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = content;

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Transcription failed: {Error}", errorContent);
            throw new Exception($"Transcription failed: {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var transcriptionResponse = JsonSerializer.Deserialize<WhisperTranscriptionResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (transcriptionResponse == null)
        {
            throw new Exception("Failed to parse transcription response");
        }

        var result = new TranscriptionResult
        {
            Text = transcriptionResponse.Text ?? string.Empty
        };

        // Convert Whisper segments to our format
        if (transcriptionResponse.Segments != null && transcriptionResponse.Segments.Any())
        {
            result.Segments = transcriptionResponse.Segments.Select(s => new TranscriptionSegment
            {
                Start = s.Start,
                End = s.End,
                Text = s.Text ?? string.Empty
            }).ToList();
        }

        _logger.LogInformation("Transcription completed. Text length: {Length}", result.Text.Length);
        return result;
    }

    private class WhisperTranscriptionResponse
    {
        public string? Text { get; set; }
        public List<WhisperSegment>? Segments { get; set; }
    }

    private class WhisperSegment
    {
        public double Start { get; set; }
        public double End { get; set; }
        public string? Text { get; set; }
    }
}

