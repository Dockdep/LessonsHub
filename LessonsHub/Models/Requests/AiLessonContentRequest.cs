using System.Text.Json.Serialization;

namespace LessonsHub.Models.Requests;

public class AiLessonContentRequest
{
    [JsonPropertyName("planName")]
    public string PlanName { get; set; } = string.Empty;

    [JsonPropertyName("topic")]
    public string Topic { get; set; } = string.Empty;

    [JsonPropertyName("lessonType")]
    public string LessonType { get; set; } = string.Empty;

    [JsonPropertyName("lessonTopic")]
    public string LessonTopic { get; set; } = string.Empty;

    [JsonPropertyName("keyPoints")]
    public List<string> KeyPoints { get; set; } = new();

    [JsonPropertyName("planDescription")]
    public string PlanDescription { get; set; } = string.Empty;

    [JsonPropertyName("lessonNumber")]
    public int LessonNumber { get; set; }

    [JsonPropertyName("lessonName")]
    public string LessonName { get; set; } = string.Empty;

    [JsonPropertyName("lessonDescription")]
    public string LessonDescription { get; set; } = string.Empty;

    [JsonPropertyName("language")]
    public string? Language { get; set; }
}
