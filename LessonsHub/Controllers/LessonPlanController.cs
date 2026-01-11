using LessonsHub.Data;
using LessonsHub.Models;
using LessonsHub.Services;
using Microsoft.AspNetCore.Mvc;

namespace LessonsHub.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LessonPlanController : ControllerBase
{
    private readonly IGeminiJsonService _geminiJsonService;
    private readonly IPromptService _promptService;
    private readonly LessonsHubDbContext _dbContext;
    private readonly ILogger<LessonPlanController> _logger;

    public LessonPlanController(
        IGeminiJsonService geminiJsonService,
        IPromptService promptService,
        LessonsHubDbContext dbContext,
        ILogger<LessonPlanController> logger)
    {
        _geminiJsonService = geminiJsonService;
        _promptService = promptService;
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpPost("generate")]
    public async Task<ActionResult<LessonPlanResponseDto>> GenerateLessonPlan([FromBody] LessonPlanRequestDto request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.PlanName) ||
                string.IsNullOrWhiteSpace(request.Topic) ||
                request.NumberOfDays < 1)
            {
                return BadRequest(new { message = "Invalid input. Please provide plan name, topic, and valid number of days." });
            }

            // Load the prompt from the external template file
            var prompt = _promptService.GetLessonPlanPrompt(
                request.PlanName,
                request.Topic,
                request.NumberOfDays,
                request.Description);

            // Send to Gemini and parse JSON response
            var lessonPlan = await _geminiJsonService.SendMessageAndParseJsonAsync<LessonPlanResponseDto>(prompt);

            if (lessonPlan == null)
            {
                return StatusCode(500, new { message = "Failed to parse lesson plan from AI response." });
            }

            return Ok(lessonPlan);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating lesson plan");
            return StatusCode(500, new { message = "An error occurred while generating the lesson plan.", error = ex.Message });
        }
    }

    [HttpPost("save")]
    public async Task<ActionResult> SaveLessonPlan([FromBody] SaveLessonPlanRequestDto request)
    {
        try
        {
            if (request == null || request.LessonPlan == null || request.LessonPlan.Lessons == null || !request.LessonPlan.Lessons.Any())
            {
                return BadRequest(new { message = "Invalid lesson plan data." });
            }

            // Create LessonPlan
            var lessonPlan = new LessonPlan
            {
                Name = request.LessonPlan.PlanName,
                Topic = request.LessonPlan.Topic,
                Description = request.Description ?? string.Empty,
                CreatedDate = DateTime.UtcNow
            };

            _dbContext.LessonPlans.Add(lessonPlan);
            await _dbContext.SaveChangesAsync();

            // Create Lessons
            foreach (var lessonDto in request.LessonPlan.Lessons)
            {
                var lesson = new Lesson
                {
                    LessonNumber = lessonDto.LessonNumber,
                    Name = lessonDto.Name,
                    ShortDescription = lessonDto.ShortDescription,
                    Content = string.Empty, // Will be generated later
                    GeminiPrompt = string.Empty,
                    LessonPlanId = lessonPlan.Id,
                    LessonDayId = null // Can be assigned to a day later
                };

                _dbContext.Lessons.Add(lesson);
            }

            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Lesson plan saved successfully.", lessonPlanId = lessonPlan.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving lesson plan");
            return StatusCode(500, new { message = "An error occurred while saving the lesson plan.", error = ex.Message });
        }
    }
}
