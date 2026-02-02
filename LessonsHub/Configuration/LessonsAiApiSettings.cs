namespace LessonsHub.Configuration;

public class LessonsAiApiSettings
{
    public string BaseUrl { get; set; } = "http://localhost:8000";
    public int TimeoutMinutes { get; set; } = 5;
}
