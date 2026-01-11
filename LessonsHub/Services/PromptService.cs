namespace LessonsHub.Services;

public interface IPromptService
{
	string GetLessonPlanPrompt(string planName, string topic, int numberOfDays, string description);

	// Added new method signature
	string GetLessonContentPrompt(string planName, string topic, string planDescription, int lessonNumber, string lessonName, string lessonDescription);
}

public class PromptService : IPromptService
{
	private readonly IWebHostEnvironment _environment;
	private readonly ILogger<PromptService> _logger;

	private string? _lessonPlanPromptTemplate;
	private string? _lessonContentPromptTemplate; // Added new field

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

	// Added new method implementation
	public string GetLessonContentPrompt(string planName, string topic, string planDescription, int lessonNumber, string lessonName, string lessonDescription)
	{
		if (_lessonContentPromptTemplate == null)
		{
			LoadLessonContentPromptTemplate();
		}

		return _lessonContentPromptTemplate!
			.Replace("{planName}", planName)
			.Replace("{topic}", topic)
			.Replace("{planDescription}", planDescription)
			.Replace("{lessonNumber}", lessonNumber.ToString())
			.Replace("{lessonName}", lessonName)
			.Replace("{lessonDescription}", lessonDescription);
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
			_lessonPlanPromptTemplate = GetFallbackPrompt();
		}
	}

	// Added new loader
	private void LoadLessonContentPromptTemplate()
	{
		try
		{
			var promptPath = Path.Combine(_environment.ContentRootPath, "Prompts", "LessonContentGenerationPrompt.txt");
			_lessonContentPromptTemplate = File.ReadAllText(promptPath);
			_logger.LogInformation("Lesson content prompt template loaded successfully");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error loading lesson content prompt template");
			// Fallback content if file is missing
			_lessonContentPromptTemplate = @"
				Course: {planName} ({topic})
				Lesson {lessonNumber}: {lessonName}
				Goal: {lessonDescription}
				Generate detailed educational content in Markdown for this lesson.";
		}
	}

	private string GetFallbackPrompt()
	{
		// ... (Existing fallback code) ...
		return @"Generate a {numberOfDays}-day lesson plan...";
	}
}