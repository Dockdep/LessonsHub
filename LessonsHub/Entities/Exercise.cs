namespace LessonsHub.Entities;

public class Exercise
{
    public int Id { get; set; }
    public string ExerciseText { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;

    public int LessonId { get; set; }
    public Lesson Lesson { get; set; } = null!;

    public List<ExerciseAnswer> Answers { get; set; } = new();
    public List<ChatMessage> ChatHistory { get; set; } = new();
}
