using LessonsHub.Data;
using LessonsHub.Models;
using LessonsHub.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LessonsHub.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class LessonController : ControllerBase
	{
		private readonly LessonsHubDbContext _dbContext;
		private readonly IGeminiService _geminiService;
		private readonly IPromptService _promptService;
		private readonly ILogger<LessonController> _logger;

		public LessonController(
			LessonsHubDbContext dbContext,
			IGeminiService geminiService,
			IPromptService promptService,
			ILogger<LessonController> logger)
		{
			_dbContext = dbContext;
			_geminiService = geminiService;
			_promptService = promptService;
			_logger = logger;
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<Lesson>> GetLesson(int id)
		{
			var lesson = await _dbContext.Lessons
				.Include(l => l.Exercises)
				.Include(l => l.LessonPlan)
				.FirstOrDefaultAsync(l => l.Id == id);

			if (lesson == null) return NotFound();

			// Check if content is missing
			if (string.IsNullOrWhiteSpace(lesson.Content))
			{
				_logger.LogInformation("Generating content for Lesson {Id}...", id);

				try
				{
					// 1. Prepare Prompt
					var planName = lesson.LessonPlan?.Name ?? "General Course";
					var planDescription = lesson.LessonPlan?.Description ?? "";

					var prompt = _promptService.GetLessonContentPrompt(
						planName: planName,
						topic: planName,
						planDescription: planDescription,
						lessonNumber: lesson.LessonNumber,
						lessonName: lesson.Name,
						lessonDescription: lesson.ShortDescription ?? ""
					);

					// 2. Prepare Request
					var geminiRequest = new GeminiRequest();
					geminiRequest.Messages.Add(new Message
					{
						Role = "user",
						Content = prompt
					});

					// 3. Call Service
					// Your service returns a flattened GeminiResponse where .Content IS the text
					var response = await _geminiService.SendMessageAsync(geminiRequest);

					if (!string.IsNullOrWhiteSpace(response.Content))
					{
						lesson.Content = response.Content; // Simple assignment!

						await _dbContext.SaveChangesAsync();
						_logger.LogInformation("Content generated and saved for Lesson {Id}", id);
					}
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error generating content for Lesson {Id}", id);
					// Swallow error so user still sees the lesson structure even if AI fails
				}
			}

			return Ok(lesson);
		}
	}
}