namespace LessonsHub.Models;

public class LessonPlanRequestDto
{
    public string PlanName { get; set; } = string.Empty;
    public int NumberOfDays { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class GeneratedLessonDto
{
    public int LessonNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
}

public class LessonPlanResponseDto
{
    public string PlanName { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public List<GeneratedLessonDto> Lessons { get; set; } = new();
}

public class SaveLessonPlanRequestDto
{
    public LessonPlanResponseDto LessonPlan { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
}
