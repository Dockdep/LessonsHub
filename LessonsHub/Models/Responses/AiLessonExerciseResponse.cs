using System.Text.Json.Serialization;

namespace LessonsHub.Models.Responses;

public class AiLessonExerciseResponse
{
    [JsonPropertyName("lessonNumber")]
    public int LessonNumber { get; set; }

    [JsonPropertyName("lessonName")]
    public string LessonName { get; set; } = string.Empty;

    [JsonPropertyName("exercise")]
    public string Exercise { get; set; } = string.Empty;
}
