using System.Text.Json.Serialization;

namespace LessonsHub.Models.Responses;

public class AiLessonPlanResponse
{
    [JsonPropertyName("planName")]
    public string PlanName { get; set; } = string.Empty;

    [JsonPropertyName("topic")]
    public string Topic { get; set; } = string.Empty;

    [JsonPropertyName("lessons")]
    public List<AiLessonItem> Lessons { get; set; } = new();
}

public class AiLessonItem
{
    [JsonPropertyName("lessonNumber")]
    public int LessonNumber { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("shortDescription")]
    public string ShortDescription { get; set; } = string.Empty;

    [JsonPropertyName("lessonTopic")]
    public string LessonTopic { get; set; } = string.Empty;

    [JsonPropertyName("keyPoints")]
    public List<string> KeyPoints { get; set; } = new();
}
