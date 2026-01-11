namespace LessonsHub.Services;

public interface IPromptService
{
    string GetLessonPlanPrompt(string planName, string topic, int numberOfDays, string description);
}

public class PromptService : IPromptService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<PromptService> _logger;
    private string? _lessonPlanPromptTemplate;

    public PromptService(IWebHostEnvironment environment, ILogger<PromptService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public string GetLessonPlanPrompt(string planName, string topic, int numberOfDays, string description)
    {
        if (_lessonPlanPromptTemplate == null)
        {
            LoadLessonPlanPromptTemplate();
        }

        var descriptionText = string.IsNullOrWhiteSpace(description)
            ? string.Empty
            : $"Additional context: {description}";

        return _lessonPlanPromptTemplate!
            .Replace("{planName}", planName)
            .Replace("{topic}", topic)
            .Replace("{numberOfDays}", numberOfDays.ToString())
            .Replace("{description}", descriptionText);
    }

    private void LoadLessonPlanPromptTemplate()
    {
        try
        {
            var promptPath = Path.Combine(_environment.ContentRootPath, "Prompts", "LessonPlanGenerationPrompt.txt");
            _lessonPlanPromptTemplate = File.ReadAllText(promptPath);
            _logger.LogInformation("Lesson plan prompt template loaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading lesson plan prompt template");
            // Fallback to inline prompt if file can't be loaded
            _lessonPlanPromptTemplate = GetFallbackPrompt();
        }
    }

    private string GetFallbackPrompt()
    {
        return @"Generate a {numberOfDays}-day lesson plan for learning {topic}.
The lesson plan name is: {planName}

{description}

For each day, provide:
1. Day number (1 to {numberOfDays})
2. A descriptive name for the lesson
3. A short description (1-2 sentences)

Return the response ONLY as a valid JSON object with this exact structure:
{
  ""planName"": ""{planName}"",
  ""topic"": ""{topic}"",
  ""lessons"": [
    {
      ""dayNumber"": 1,
      ""name"": ""Lesson name"",
      ""shortDescription"": ""Brief description"",
      ""topic"": ""{topic}""
    }
  ]
}

IMPORTANT: Return ONLY the JSON object, no additional text or markdown formatting.";
    }
}
