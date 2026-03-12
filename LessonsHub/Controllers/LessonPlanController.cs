using LessonsHub.Infrastructure.Data;
using LessonsHub.Domain.Entities;
using LessonsHub.Application.Interfaces;
using LessonsHub.Application.Models.Requests;
using LessonsHub.Application.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LessonsHub.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
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

    private int GetCurrentUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("{id}")]
    public async Task<ActionResult<LessonPlanDetailDto>> GetLessonPlanDetail(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var lessonPlan = await _dbContext.LessonPlans
                .Include(lp => lp.Lessons)
                .FirstOrDefaultAsync(lp => lp.Id == id && lp.UserId == userId);

            if (lessonPlan == null)
                return NotFound(new { message = "Lesson plan not found." });

            var detail = new LessonPlanDetailDto
            {
                Id = lessonPlan.Id,
                Name = lessonPlan.Name,
                Topic = lessonPlan.Topic,
                Description = lessonPlan.Description,
                NativeLanguage = lessonPlan.NativeLanguage,
                CreatedDate = lessonPlan.CreatedDate,
                Lessons = lessonPlan.Lessons
                    .OrderBy(l => l.LessonNumber)
                    .Select(l => new PlanLessonDto
                    {
                        Id = l.Id,
                        LessonNumber = l.LessonNumber,
                        Name = l.Name,
                        ShortDescription = l.ShortDescription,
                        LessonTopic = l.LessonTopic,
                        IsCompleted = l.IsCompleted
                    })
                    .ToList()
            };

            return Ok(detail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving lesson plan detail");
            return StatusCode(500, new { message = "An error occurred while retrieving the lesson plan." });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteLessonPlan(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var lessonPlan = await _dbContext.LessonPlans
                .Include(lp => lp.Lessons)
                .FirstOrDefaultAsync(lp => lp.Id == id && lp.UserId == userId);

            if (lessonPlan == null)
                return NotFound(new { message = "Lesson plan not found." });

            var affectedDayIds = lessonPlan.Lessons
                .Where(l => l.LessonDayId != null)
                .Select(l => l.LessonDayId!.Value)
                .Distinct()
                .ToList();

            _dbContext.LessonPlans.Remove(lessonPlan);
            await _dbContext.SaveChangesAsync();

            if (affectedDayIds.Count > 0)
            {
                var emptyDays = await _dbContext.LessonDays
                    .Where(ld => affectedDayIds.Contains(ld.Id) && !ld.Lessons.Any())
                    .ToListAsync();

                if (emptyDays.Count > 0)
                {
                    _dbContext.LessonDays.RemoveRange(emptyDays);
                    await _dbContext.SaveChangesAsync();
                }
            }

            return Ok(new { message = "Lesson plan deleted successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting lesson plan");
            return StatusCode(500, new { message = "An error occurred while deleting the lesson plan." });
        }
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

            var aiRequest = new AiLessonPlanRequest
            {
                LessonType = request.LessonType,
                Topic = request.Topic,
                NumberOfLessons = request.NumberOfDays,
                Description = request.Description,
                Language = request.NativeLanguage
            };

            var aiResponse = await _aiApiClient.GenerateLessonPlanAsync(aiRequest);

            if (aiResponse == null)
                return StatusCode(500, new { message = "Failed to get lesson plan from AI API." });

            var lessonPlan = new LessonPlanResponseDto
            {
                PlanName = request.PlanName, // Use the one from the incoming HTTP request DTO
                Topic = aiResponse.Topic,
                Lessons = aiResponse.Lessons.Select(l => new GeneratedLessonDto
                {
                    LessonNumber = l.LessonNumber,
                    Name = l.Name,
                    ShortDescription = l.ShortDescription,
                    LessonTopic = l.LessonTopic,
                    KeyPoints = l.KeyPoints
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
            if (request?.LessonPlan?.Lessons == null || !request.LessonPlan.Lessons.Any())
                return BadRequest(new { message = "Invalid lesson plan data." });

            var userId = GetCurrentUserId();

            var lessonPlan = new LessonPlan
            {
                Name = request.LessonPlan.PlanName,
                Topic = request.LessonPlan.Topic,
                Description = request.Description ?? string.Empty,
                NativeLanguage = request.NativeLanguage,
                CreatedDate = DateTime.UtcNow,
                UserId = userId
            };

            _dbContext.LessonPlans.Add(lessonPlan);
            await _dbContext.SaveChangesAsync();

            foreach (var lessonDto in request.LessonPlan.Lessons)
            {
                var lesson = new Lesson
                {
                    LessonNumber = lessonDto.LessonNumber,
                    Name = lessonDto.Name,
                    ShortDescription = lessonDto.ShortDescription,
                    Content = string.Empty,
                    LessonType = request.LessonType ?? string.Empty,
                    LessonPlanId = lessonPlan.Id,
                    LessonTopic = lessonDto.LessonTopic,
                    KeyPoints = lessonDto.KeyPoints ?? new(),
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
