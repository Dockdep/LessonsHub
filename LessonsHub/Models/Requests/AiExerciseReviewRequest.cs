using System.Text.Json.Serialization;

namespace LessonsHub.Models.Requests;

public class AiExerciseReviewRequest
{
    [JsonPropertyName("lessonType")]
    public string LessonType { get; set; } = string.Empty;

    [JsonPropertyName("lessonContent")]
    public string LessonContent { get; set; } = string.Empty;

    [JsonPropertyName("exerciseContent")]
    public string ExerciseContent { get; set; } = string.Empty;

    [JsonPropertyName("difficulty")]
    public string Difficulty { get; set; } = string.Empty;

    [JsonPropertyName("answer")]
    public string Answer { get; set; } = string.Empty;
}
