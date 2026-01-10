namespace LessonsHub.Models;

public class GeminiRequest
{
    public List<Message> Messages { get; set; } = new();
}

public class Message
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
