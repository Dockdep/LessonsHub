namespace LessonsHub.Models;

public class LessonDayDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public List<AssignedLessonDto> Lessons { get; set; } = new();
}

public class AssignedLessonDto
{
    public int Id { get; set; }
    public int LessonNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public int LessonPlanId { get; set; }
    public string LessonPlanName { get; set; } = string.Empty;
}

public class LessonPlanSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public int LessonsCount { get; set; }
}

public class AvailableLessonDto
{
    public int Id { get; set; }
    public int LessonNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public int LessonPlanId { get; set; }
    public string LessonPlanName { get; set; } = string.Empty;
    public bool IsAssigned { get; set; }
}

public class AssignLessonRequestDto
{
    public int LessonId { get; set; }
    public string Date { get; set; } = string.Empty;
    public string DayName { get; set; } = string.Empty;
    public string DayDescription { get; set; } = string.Empty;
}
