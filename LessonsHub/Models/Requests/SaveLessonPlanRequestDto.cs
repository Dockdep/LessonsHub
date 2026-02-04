using LessonsHub.Models.Responses;

namespace LessonsHub.Models.Requests;

public class SaveLessonPlanRequestDto
{
    public LessonPlanResponseDto LessonPlan { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public string? LessonType { get; set; }
}
