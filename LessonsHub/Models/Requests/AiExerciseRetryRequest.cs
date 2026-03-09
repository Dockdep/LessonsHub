using System.Text.Json.Serialization;

namespace LessonsHub.Models.Requests;

public class AiExerciseRetryRequest
{
    [JsonPropertyName("planName")]
    public string PlanName { get; set; } = string.Empty;

    [JsonPropertyName("planTopic")]
    public string PlanTopic { get; set; } = string.Empty;

    [JsonPropertyName("planDescription")]
    public string PlanDescription { get; set; } = string.Empty;

    [JsonPropertyName("lessonType")]
    public string LessonType { get; set; } = string.Empty;

    [JsonPropertyName("lessonTopic")]
    public string LessonTopic { get; set; } = string.Empty;

    [JsonPropertyName("lessonNumber")]
    public int LessonNumber { get; set; }

    [JsonPropertyName("lessonName")]
    public string LessonName { get; set; } = string.Empty;

    [JsonPropertyName("lessonDescription")]
    public string LessonDescription { get; set; } = string.Empty;

    [JsonPropertyName("lessonContent")]
    public string LessonContent { get; set; } = string.Empty;

    [JsonPropertyName("keyPoints")]
    public List<string> KeyPoints { get; set; } = new();

    [JsonPropertyName("difficulty")]
    public string Difficulty { get; set; } = string.Empty;

    [JsonPropertyName("review")]
    public string Review { get; set; } = string.Empty;

    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    [JsonPropertyName("nativeLanguage")]
    public string? NativeLanguage { get; set; }
}
