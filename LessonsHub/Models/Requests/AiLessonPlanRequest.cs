using System.Text.Json.Serialization;

namespace LessonsHub.Models.Requests;

public class AiLessonPlanRequest
{
	[JsonPropertyName("lessonType")]
	public string LessonType { get; set; } = string.Empty;
	
    [JsonPropertyName("planName")]
    public string PlanName { get; set; } = string.Empty;

    [JsonPropertyName("topic")]
    public string Topic { get; set; } = string.Empty;

    [JsonPropertyName("numberOfLessons")]
    public int? NumberOfLessons { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}
