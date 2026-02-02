using System.Text.Json;
using LessonsHub.Models;

namespace LessonsHub.Services;

public interface IGeminiJsonService
{
    Task<T?> SendMessageAndParseJsonAsync<T>(string prompt) where T : class;
}

public class GeminiJsonService : IGeminiJsonService
{
    private readonly IGeminiService _geminiService;
    private readonly ILogger<GeminiJsonService> _logger;
    private readonly IConfiguration _configuration;

    public GeminiJsonService(IGeminiService geminiService, ILogger<GeminiJsonService> logger)
    {
        _geminiService = geminiService;
        _logger = logger;
    }

    public async Task<T?> SendMessageAndParseJsonAsync<T>(string prompt) where T : class
    {
        try
        {
            // Check if mock mode is enabled
            var useMockResponse = _configuration.GetValue<bool>("UseMockGeminiResponse");

            // Create Gemini request
            var geminiRequest = new GeminiRequest
            {
                Messages = new List<Message>
                {
                    new Message { Role = "user", Content = prompt }
                }
            };

            // Send to Gemini
            var geminiResponse = await _geminiService.SendMessageAsync(geminiRequest);

			// Clean up the response content
			string responseContent = CleanJsonResponse(geminiResponse.Content);
            

            _logger.LogDebug("Cleaned response content: {Content}", responseContent);

            // Deserialize to the requested type
            var result = JsonSerializer.Deserialize<T>(
                responseContent,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return result;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse JSON response from Gemini. Content: {Content}", ex.Message);
            throw new InvalidOperationException("Failed to parse AI response as JSON", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SendMessageAndParseJsonAsync");
            throw;
        }
    }

    private string CleanJsonResponse(string content)
    {
        var cleaned = content.Trim();

        // Remove markdown code blocks if present
        if (cleaned.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
        {
            cleaned = cleaned.Substring(7);
        }
        else if (cleaned.StartsWith("```"))
        {
            cleaned = cleaned.Substring(3);
        }

        if (cleaned.EndsWith("```"))
        {
            cleaned = cleaned.Substring(0, cleaned.Length - 3);
        }

        return cleaned.Trim();
    }
}
