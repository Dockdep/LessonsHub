using System.Net.Http.Json;
using LessonsHub.Interfaces;
using LessonsHub.Models.Requests;
using LessonsHub.Models.Responses;

namespace LessonsHub.Services;

public class LessonsAiApiClient : ILessonsAiApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LessonsAiApiClient> _logger;

    public LessonsAiApiClient(HttpClient httpClient, ILogger<LessonsAiApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<AiLessonPlanResponse?> GenerateLessonPlanAsync(AiLessonPlanRequest request)
    {
        try
        {
            _logger.LogInformation("Generating lesson plan for topic: {Topic}", request.Topic);

            var response = await _httpClient.PostAsJsonAsync("/api/lesson-plan/generate", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("AI API Error: {StatusCode} - {Error}", response.StatusCode, error);
                throw new HttpRequestException($"AI API Error: {response.StatusCode} - {error}");
            }

            return await response.Content.ReadFromJsonAsync<AiLessonPlanResponse>();
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("AI API request timed out for topic: {Topic}", request.Topic);
            throw new TimeoutException("The AI API request timed out. Please try again.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to AI API");
            throw new Exception($"Failed to connect to AI API: {ex.Message}");
        }
    }

    public async Task<AiLessonContentResponse?> GenerateLessonContentAsync(AiLessonContentRequest request)
    {
        try
        {
            _logger.LogInformation("Generating content for lesson: {LessonName}", request.LessonName);

            var response = await _httpClient.PostAsJsonAsync("/api/lesson-content/generate", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("AI API Error: {StatusCode} - {Error}", response.StatusCode, error);
                throw new HttpRequestException($"AI API Error: {response.StatusCode} - {error}");
            }

            return await response.Content.ReadFromJsonAsync<AiLessonContentResponse>();
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("AI API request timed out for lesson: {LessonName}", request.LessonName);
            throw new TimeoutException("The AI API request timed out. Please try again.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to AI API");
            throw new Exception($"Failed to connect to AI API: {ex.Message}");
        }
    }

    public async Task<AiLessonResourcesResponse?> GenerateLessonResourcesAsync(AiLessonResourcesRequest request)
    {
        try
        {
            _logger.LogInformation("Generating resources for lesson: {LessonName}", request.LessonName);

            var response = await _httpClient.PostAsJsonAsync("/api/lesson-resources/generate", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("AI API Error: {StatusCode} - {Error}", response.StatusCode, error);
                throw new HttpRequestException($"AI API Error: {response.StatusCode} - {error}");
            }

            return await response.Content.ReadFromJsonAsync<AiLessonResourcesResponse>();
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("AI API request timed out for lesson resources: {LessonName}", request.LessonName);
            throw new TimeoutException("The AI API request timed out. Please try again.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to AI API");
            throw new Exception($"Failed to connect to AI API: {ex.Message}");
        }
    }

    public async Task<bool> HealthCheckAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/health");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
