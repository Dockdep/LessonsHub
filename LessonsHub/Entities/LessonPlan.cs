namespace LessonsHub.Entities;

public class LessonPlan
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // One LessonPlan can have multiple Lessons
    public List<Lesson> Lessons { get; set; } = new();
}
