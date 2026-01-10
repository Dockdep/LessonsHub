using System.Text;
using System.Text.Json;
using LessonsHub.Models;

namespace LessonsHub.Services;

public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _apiKey;
    private readonly string _model;

    public GeminiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _apiKey = configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini API Key not configured");
        _model = configuration["Gemini:Model"] ?? "gemini-pro";
    }

	public async Task<GeminiResponse> SendMessageAsync(GeminiRequest request)
	{
		try
		{
			// Use the actual messages from the request
			var geminiApiRequest = new
			{
				contents = request.Messages.Select(m => new
				{
					role = m.Role,
					parts = new[] { new { text = m.Content } }
				}).ToList()
			};

			var jsonContent = JsonSerializer.Serialize(geminiApiRequest);
			var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

			var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

			var response = await _httpClient.PostAsync(url, content);

			if (!response.IsSuccessStatusCode)
			{
				var errorBody = await response.Content.ReadAsStringAsync();
				throw new Exception($"API Error ({response.StatusCode}): {errorBody}");
			}

			var responseContent = await response.Content.ReadAsStringAsync();
			var geminiResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

			var text = geminiResponse.GetProperty("candidates")[0]
									 .GetProperty("content")
									 .GetProperty("parts")[0]
									 .GetProperty("text").GetString();

			var tokensUsed = 0;
			if (geminiResponse.TryGetProperty("usageMetadata", out var usageMetadata))
			{
				if (usageMetadata.TryGetProperty("totalTokenCount", out var tokenCount))
				{
					tokensUsed = tokenCount.GetInt32();
				}
			}

			return new GeminiResponse
			{
				Content = text ?? string.Empty,
				Model = _model,
				TokensUsed = tokensUsed
			};
		}
		catch (HttpRequestException ex)
		{
			throw new Exception($"Error communicating with Gemini API: {ex.Message}", ex);
		}
		catch (JsonException ex)
		{
			throw new Exception($"Error parsing Gemini API response: {ex.Message}", ex);
		}
	}
}
