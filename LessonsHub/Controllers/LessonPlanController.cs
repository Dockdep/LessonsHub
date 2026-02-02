using LessonsHub.Data;
using LessonsHub.Entities;
using LessonsHub.Interfaces;
using LessonsHub.Models.Requests;
using LessonsHub.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace LessonsHub.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LessonPlanController : ControllerBase
{
    private readonly ILessonsAiApiClient _aiApiClient;
    private readonly LessonsHubDbContext _dbContext;
    private readonly ILogger<LessonPlanController> _logger;

    public LessonPlanController(
        ILessonsAiApiClient aiApiClient,
        LessonsHubDbContext dbContext,
        ILogger<LessonPlanController> logger)
    {
        _aiApiClient = aiApiClient;
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpPost("generate")]
    public async Task<ActionResult<LessonPlanResponseDto>> GenerateLessonPlan([FromBody] LessonPlanRequestDto request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.PlanName) ||
                string.IsNullOrWhiteSpace(request.Topic))
            {
                return BadRequest(new { message = "Invalid input. Please provide plan name and topic." });
            }

            // Call the AI API
            var aiRequest = new AiLessonPlanRequest
            {
                PlanName = request.PlanName,
                Topic = request.Topic,
                NumberOfLessons = request.NumberOfDays,
                Description = request.Description
            };

            var aiResponse = await _aiApiClient.GenerateLessonPlanAsync(aiRequest);

            if (aiResponse == null)
            {
                return StatusCode(500, new { message = "Failed to get lesson plan from AI API." });
            }

            // Map AI response to our DTO
            var lessonPlan = new LessonPlanResponseDto
            {
                PlanName = aiResponse.PlanName,
                Topic = aiResponse.Topic,
                Lessons = aiResponse.Lessons.Select(l => new GeneratedLessonDto
                {
                    LessonNumber = l.LessonNumber,
                    Name = l.Name,
                    ShortDescription = l.ShortDescription,
                    Topic = l.Topic
                }).ToList()
            };

            return Ok(lessonPlan);
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "Timeout generating lesson plan");
            return StatusCode(504, new { message = "The AI service is taking too long. Please try again." });
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
                    Content = string.Empty,
                    GeminiPrompt = string.Empty,
                    LessonPlanId = lessonPlan.Id,
                    LessonDayId = null
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
