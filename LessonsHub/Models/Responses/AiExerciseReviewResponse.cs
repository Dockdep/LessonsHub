using System.Text.Json.Serialization;

namespace LessonsHub.Models.Responses;

public class AiExerciseReviewResponse
{
    [JsonPropertyName("accuracyLevel")]
    public int AccuracyLevel { get; set; }

    [JsonPropertyName("examReview")]
    public string ExamReview { get; set; } = string.Empty;
}
