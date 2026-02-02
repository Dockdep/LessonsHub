using System.Text.Json.Serialization;

namespace LessonsHub.Models.Responses;

public class AiLessonContentResponse
{
    [JsonPropertyName("lessonNumber")]
    public int LessonNumber { get; set; }

    [JsonPropertyName("lessonName")]
    public string LessonName { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}
