using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using LessonsHub.Application.Interfaces;
using LessonsHub.Application.Models.Requests;
using LessonsHub.Application.Models.Responses;
using LessonsHub.Domain.Entities;
using LessonsHub.Infrastructure.Data;
using LessonsHub.Infrastructure.Configuration;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace LessonsHub.Infrastructure.Services;

public class LessonsAiApiClient : ILessonsAiApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LessonsAiApiClient> _logger;
    private readonly LessonsHubDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LessonsAiApiSettings _aiApiSettings;

    public LessonsAiApiClient(
        HttpClient httpClient, 
        ILogger<LessonsAiApiClient> logger,
        LessonsHubDbContext dbContext,
        IHttpContextAccessor httpContextAccessor,
        LessonsAiApiSettings aiApiSettings)
    {
        _httpClient = httpClient;
        _logger = logger;
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _aiApiSettings = aiApiSettings;
    }

    private async Task LogModelUsageAsync(List<ModelUsage> usageList, Guid correlationId)
    {
        if (usageList == null || !usageList.Any()) return;

        var userIdString = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        int? userId = int.TryParse(userIdString, out var parsedId) ? parsedId : null;

        foreach (var usage in usageList)
        {
            var log = new AiRequestLog
            {
                UserId = userId,
                CorrelationId = correlationId,
                RequestType = usage.RequestType ?? string.Empty,
                ModelName = usage.ModelName ?? string.Empty,
                InputTokens = usage.InputTokens,
                OutputTokens = usage.OutputTokens,
                LatencyMs = usage.LatencyMs,
                IsSuccess = usage.IsSuccess,
                FinishReason = usage.FinishReason ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            // Calculate cost using hardcoded settings
            decimal pricePerIn = 0;
            decimal pricePerOut = 0;
            
            var modelNameLower = usage.ModelName?.ToLower() ?? "";
            var pricing = _aiApiSettings.Pricing;

            if (modelNameLower.Contains("pro"))
            {
                if (usage.InputTokens <= 200000)
                {
                    pricePerIn = pricing.GeminiProPreviewInputTokenPriceUnder200k;
                    pricePerOut = pricing.GeminiProPreviewOutputTokenPriceUnder200k;
                }
                else
                {
                    pricePerIn = pricing.GeminiProPreviewInputTokenPriceOver200k;
                    pricePerOut = pricing.GeminiProPreviewOutputTokenPriceOver200k;
                }
            }
            else if (modelNameLower.Contains("flash"))
            {
                pricePerIn = pricing.GeminiFlashPreviewInputTokenPrice;
                pricePerOut = pricing.GeminiFlashPreviewOutputTokenPrice;
            }

            log.PricePerIn = pricePerIn;
            log.PricePerOut = pricePerOut;
            log.TotalCost = (log.InputTokens * log.PricePerIn) + (log.OutputTokens * log.PricePerOut);

            _dbContext.AiRequestLogs.Add(log);
        }

        await _dbContext.SaveChangesAsync();
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

            var responseData = await response.Content.ReadFromJsonAsync<AiLessonPlanResponse>();
            if (responseData?.Usage != null)
            {
                var correlationId = Guid.TryParse(responseData.CorrelationId, out var cid) ? cid : Guid.NewGuid();
                await LogModelUsageAsync(responseData.Usage, correlationId);
            }
            return responseData;
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

            var responseData = await response.Content.ReadFromJsonAsync<AiLessonContentResponse>();
            if (responseData?.Usage != null)
            {
                var correlationId = Guid.TryParse(responseData.CorrelationId, out var cid) ? cid : Guid.NewGuid();
                await LogModelUsageAsync(responseData.Usage, correlationId);
            }
            return responseData;
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

    public async Task<AiLessonExerciseResponse?> GenerateLessonExerciseAsync(AiLessonExerciseRequest request)
    {
        try
        {
            _logger.LogInformation("Generating exercise for lesson: {LessonName}", request.LessonName);

            var response = await _httpClient.PostAsJsonAsync("/api/lesson-exercise/generate", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("AI API Error: {StatusCode} - {Error}", response.StatusCode, error);
                throw new HttpRequestException($"AI API Error: {response.StatusCode} - {error}");
            }

            var responseData = await response.Content.ReadFromJsonAsync<AiLessonExerciseResponse>();
            if (responseData?.Usage != null)
            {
                var correlationId = Guid.TryParse(responseData.CorrelationId, out var cid) ? cid : Guid.NewGuid();
                await LogModelUsageAsync(responseData.Usage, correlationId);
            }
            return responseData;
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("AI API request timed out for lesson exercise: {LessonName}", request.LessonName);
            throw new TimeoutException("The AI API request timed out. Please try again.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to AI API");
            throw new Exception($"Failed to connect to AI API: {ex.Message}");
        }
    }

    public async Task<AiLessonExerciseResponse?> RetryLessonExerciseAsync(AiExerciseRetryRequest request)
    {
        try
        {
            _logger.LogInformation("Retrying exercise for lesson: {LessonName}", request.LessonName);

            var response = await _httpClient.PostAsJsonAsync("/api/lesson-exercise/retry", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("AI API Error: {StatusCode} - {Error}", response.StatusCode, error);
                throw new HttpRequestException($"AI API Error: {response.StatusCode} - {error}");
            }

            var responseData = await response.Content.ReadFromJsonAsync<AiLessonExerciseResponse>();
            if (responseData?.Usage != null)
            {
                var correlationId = Guid.TryParse(responseData.CorrelationId, out var cid) ? cid : Guid.NewGuid();
                await LogModelUsageAsync(responseData.Usage, correlationId);
            }
            return responseData;
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("AI API request timed out for exercise retry: {LessonName}", request.LessonName);
            throw new TimeoutException("The AI API request timed out. Please try again.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to AI API");
            throw new Exception($"Failed to connect to AI API: {ex.Message}");
        }
    }

    public async Task<AiExerciseReviewResponse?> CheckExerciseReviewAsync(AiExerciseReviewRequest request)
    {
        try
        {
            _logger.LogInformation("Checking exercise review");

            var response = await _httpClient.PostAsJsonAsync("/api/exercise-review/check", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("AI API Error: {StatusCode} - {Error}", response.StatusCode, error);
                throw new HttpRequestException($"AI API Error: {response.StatusCode} - {error}");
            }

            var responseData = await response.Content.ReadFromJsonAsync<AiExerciseReviewResponse>();
            if (responseData?.Usage != null)
            {
                var correlationId = Guid.TryParse(responseData.CorrelationId, out var cid) ? cid : Guid.NewGuid();
                await LogModelUsageAsync(responseData.Usage, correlationId);
            }
            return responseData;
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("AI API request timed out for exercise review");
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

            var responseData = await response.Content.ReadFromJsonAsync<AiLessonResourcesResponse>();
            if (responseData?.Usage != null)
            {
                var correlationId = Guid.TryParse(responseData.CorrelationId, out var cid) ? cid : Guid.NewGuid();
                await LogModelUsageAsync(responseData.Usage, correlationId);
            }
            return responseData;
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
