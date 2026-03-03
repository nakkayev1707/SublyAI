using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SublyAI.Services;

public class TranslationService : ITranslationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TranslationService> _logger;
    private readonly HttpClient _httpClient;

    public TranslationService(IConfiguration configuration, ILogger<TranslationService> logger, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<string> TranslateAsync(string text, string targetLanguage)
    {
        var apiKey = _configuration["OpenAI:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new Exception("OpenAI API key is not configured");
        }

        var prompt = $"Translate the following text to {targetLanguage}. Only return the translated text, no explanations:\n\n{text}";

        var requestBody = new
        {
            model = _configuration["OpenAI:Model"] ?? "gpt-4",
            messages = new[]
            {
                new { role = "system", content = "You are a professional translator. Translate the given text accurately while preserving the meaning and tone." },
                new { role = "user", content = prompt }
            }
        };

        var jsonContent = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Content = content;

        _logger.LogInformation("Sending translation request for language: {Language}", targetLanguage);
        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Translation failed: {Error}", errorContent);
            throw new Exception($"Translation failed: {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var chatResponse = JsonSerializer.Deserialize<ChatCompletionResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (chatResponse == null || chatResponse.Choices == null || !chatResponse.Choices.Any())
        {
            throw new Exception("Failed to parse translation response");
        }

        var translatedText = chatResponse.Choices[0].Message?.Content?.Trim() ?? string.Empty;
        _logger.LogInformation("Translation completed. Translated text length: {Length}", translatedText.Length);

        return translatedText;
    }

    private class ChatCompletionResponse
    {
        public List<ChatChoice>? Choices { get; set; }
    }

    private class ChatChoice
    {
        public ChatMessage? Message { get; set; }
    }

    private class ChatMessage
    {
        public string? Content { get; set; }
    }
}

