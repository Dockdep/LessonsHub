using LessonsHub.Models.Requests;
using LessonsHub.Models.Responses;

namespace LessonsHub.Interfaces;

public interface ILessonsAiApiClient
{
    Task<AiLessonPlanResponse?> GenerateLessonPlanAsync(AiLessonPlanRequest request);
    Task<AiLessonContentResponse?> GenerateLessonContentAsync(AiLessonContentRequest request);
    Task<AiLessonExerciseResponse?> GenerateLessonExerciseAsync(AiLessonExerciseRequest request);
    Task<AiExerciseReviewResponse?> CheckExerciseReviewAsync(AiExerciseReviewRequest request);
    Task<AiLessonResourcesResponse?> GenerateLessonResourcesAsync(AiLessonResourcesRequest request);
    Task<bool> HealthCheckAsync();
}
