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

    public GeminiJsonService(IGeminiService geminiService, ILogger<GeminiJsonService> logger, IConfiguration configuration)
    {
        _geminiService = geminiService;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<T?> SendMessageAndParseJsonAsync<T>(string prompt) where T : class
    {
        try
        {
            // Check if mock mode is enabled
            var useMockResponse = _configuration.GetValue<bool>("UseMockGeminiResponse");

            string responseContent;

            if (useMockResponse)
            {
                _logger.LogInformation("Using mock Gemini response");
                responseContent = GetMockResponse();
            }
            else
            {
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
                responseContent = CleanJsonResponse(geminiResponse.Content);
            }

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

    private string GetMockResponse()
    {
        return @"{
  ""planName"": ""Angular basic in 5 lessons"",
  ""topic"": ""Angular basic"",
  ""lessons"": [
    {
      ""lessonNumber"": 1,
      ""name"": ""Introduction, Setup, and Core Architecture"",
      ""shortDescription"": ""Learn what Angular is, why it's a powerful TypeScript-based framework for scalable web apps, and set up your first project using the CLI."",
      ""topic"": ""Angular basic""
    },
    {
      ""lessonNumber"": 2,
      ""name"": ""Components and Data Binding"",
      ""shortDescription"": ""Explore the building blocks of Angular and master the four types of data binding to sync data between logic and the UI."",
      ""topic"": ""Angular basic""
    },
    {
      ""lessonNumber"": 3,
      ""name"": ""Directives and Pipes"",
      ""shortDescription"": ""Learn to manipulate the DOM structure with structural directives like NgIf and NgFor, and transform data display using built-in pipes."",
      ""topic"": ""Angular basic""
    },
    {
      ""lessonNumber"": 4,
      ""name"": ""Services and Dependency Injection"",
      ""shortDescription"": ""Discover how to share data and business logic across multiple components efficiently using Angular's modular service system and dependency injection."",
      ""topic"": ""Angular basic""
    },
    {
      ""lessonNumber"": 5,
      ""name"": ""Routing and HTTP Communications"",
      ""shortDescription"": ""Build a complete Single Page Application by implementing navigation between views and fetching real-world data from external APIs."",
      ""topic"": ""Angular basic""
    }
  ]
}";
    }
}
