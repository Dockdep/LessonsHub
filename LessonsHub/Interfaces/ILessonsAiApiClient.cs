using LessonsHub.Models.Requests;
using LessonsHub.Models.Responses;

namespace LessonsHub.Interfaces;

public interface ILessonsAiApiClient
{
    Task<AiLessonPlanResponse?> GenerateLessonPlanAsync(AiLessonPlanRequest request);
    Task<AiLessonContentResponse?> GenerateLessonContentAsync(AiLessonContentRequest request);
    Task<AiLessonResourcesResponse?> GenerateLessonResourcesAsync(AiLessonResourcesRequest request);
    Task<bool> HealthCheckAsync();
}
