namespace LessonsHub.Models.Requests;

public class LessonPlanRequestDto
{
    public string LessonType { get; set; } = string.Empty;
    public string LessonTopic { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public int? NumberOfDays { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
