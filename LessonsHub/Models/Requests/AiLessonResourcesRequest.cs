using System.Text.Json.Serialization;

namespace LessonsHub.Models.Requests;

public class AiLessonResourcesRequest
{
    [JsonPropertyName("topic")]
    public string Topic { get; set; } = string.Empty;

    [JsonPropertyName("lessonName")]
    public string LessonName { get; set; } = string.Empty;

    [JsonPropertyName("lessonDescription")]
    public string LessonDescription { get; set; } = string.Empty;
}
