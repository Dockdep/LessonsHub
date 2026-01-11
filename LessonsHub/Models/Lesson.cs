namespace LessonsHub.Models;

public class Lesson
{
    public int Id { get; set; }
    public int LessonNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string GeminiPrompt { get; set; } = string.Empty;

    // Relationships
    public int LessonPlanId { get; set; }
    public LessonPlan LessonPlan { get; set; } = null!;

    public int? LessonDayId { get; set; }
    public LessonDay? LessonDay { get; set; }

    public List<Exercise> Exercises { get; set; } = new();

    // History link
    public List<ChatMessage> ChatHistory { get; set; } = new();
}
