using LessonsHub.Models;

namespace LessonsHub.Services;

public interface IGeminiService
{
    Task<GeminiResponse> SendMessageAsync(GeminiRequest request);
}
