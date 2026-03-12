using LessonsHub.Infrastructure.Data;
using LessonsHub.Domain.Entities;
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
public class LessonDayController : ControllerBase
{
    private readonly LessonsHubDbContext _dbContext;
    private readonly ILogger<LessonDayController> _logger;

    public LessonDayController(LessonsHubDbContext dbContext, ILogger<LessonDayController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    private int GetCurrentUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("plans")]
    public async Task<ActionResult<List<LessonPlanSummaryDto>>> GetLessonPlans()
    {
        try
        {
            var userId = GetCurrentUserId();
            var plans = await _dbContext.LessonPlans
                .Include(lp => lp.Lessons)
                .Where(lp => lp.UserId == userId)
                .Select(lp => new LessonPlanSummaryDto
                {
                    Id = lp.Id,
                    Name = lp.Name,
                    Topic = lp.Topic,
                    Description = lp.Description,
                    CreatedDate = lp.CreatedDate,
                    LessonsCount = lp.Lessons.Count
                })
                .OrderByDescending(lp => lp.CreatedDate)
                .ToListAsync();

            return Ok(plans);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving lesson plans");
            return StatusCode(500, new { message = "An error occurred while retrieving lesson plans." });
        }
    }

    [HttpGet("plans/{lessonPlanId}/lessons")]
    public async Task<ActionResult<List<AvailableLessonDto>>> GetAvailableLessons(int lessonPlanId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var lessonPlan = await _dbContext.LessonPlans
                .Include(lp => lp.Lessons)
                .FirstOrDefaultAsync(lp => lp.Id == lessonPlanId && lp.UserId == userId);

            if (lessonPlan == null)
                return NotFound(new { message = "Lesson plan not found." });

            var lessons = lessonPlan.Lessons
                .OrderBy(l => l.LessonNumber)
                .Select(l => new AvailableLessonDto
                {
                    Id = l.Id,
                    LessonNumber = l.LessonNumber,
                    Name = l.Name,
                    ShortDescription = l.ShortDescription,
                    LessonPlanId = lessonPlan.Id,
                    LessonPlanName = lessonPlan.Name,
                    IsAssigned = l.LessonDayId != null
                })
                .ToList();

            return Ok(lessons);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available lessons");
            return StatusCode(500, new { message = "An error occurred while retrieving lessons." });
        }
    }

    [HttpGet("{year}/{month}")]
    public async Task<ActionResult<List<LessonDayDto>>> GetLessonDaysByMonth(int year, int month)
    {
        try
        {
            var userId = GetCurrentUserId();
            var startDate = DateTime.SpecifyKind(new DateTime(year, month, 1), DateTimeKind.Utc);
            var endDate = startDate.AddMonths(1);

            var lessonDays = await _dbContext.LessonDays
                .Include(ld => ld.Lessons)
                    .ThenInclude(l => l.LessonPlan)
                .Where(ld => ld.Date >= startDate && ld.Date < endDate &&
                             ld.Lessons.Any(l => l.LessonPlan.UserId == userId))
                .OrderBy(ld => ld.Date)
                .Select(ld => new LessonDayDto
                {
                    Id = ld.Id,
                    Date = ld.Date,
                    Name = ld.Name,
                    ShortDescription = ld.ShortDescription,
                    Lessons = ld.Lessons
                        .Where(l => l.LessonPlan.UserId == userId)
                        .Select(l => new AssignedLessonDto
                        {
                            Id = l.Id,
                            LessonNumber = l.LessonNumber,
                            Name = l.Name,
                            ShortDescription = l.ShortDescription,
                            LessonPlanId = l.LessonPlanId,
                            LessonPlanName = l.LessonPlan.Name,
                            IsCompleted = l.IsCompleted
                        }).ToList()
                })
                .ToListAsync();

            return Ok(lessonDays);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving lesson days");
            return StatusCode(500, new { message = "An error occurred while retrieving lesson days." });
        }
    }

    [HttpPost("assign")]
    public async Task<ActionResult> AssignLesson([FromBody] AssignLessonRequestDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var lesson = await _dbContext.Lessons
                .Include(l => l.LessonPlan)
                .FirstOrDefaultAsync(l => l.Id == request.LessonId);

            if (lesson == null || lesson.LessonPlan?.UserId != userId)
                return NotFound(new { message = "Lesson not found." });

            if (!DateTime.TryParse(request.Date, out var date))
                return BadRequest(new { message = "Invalid date format." });

            var utcDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);

            var lessonDay = await _dbContext.LessonDays
                .FirstOrDefaultAsync(ld => ld.Date.Date == utcDate.Date);

            if (lessonDay == null)
            {
                lessonDay = new LessonDay
                {
                    Date = utcDate,
                    Name = request.DayName,
                    ShortDescription = request.DayDescription
                };
                _dbContext.LessonDays.Add(lessonDay);
            }
            else
            {
                lessonDay.Name = request.DayName;
                lessonDay.ShortDescription = request.DayDescription;
            }
            await _dbContext.SaveChangesAsync();

            lesson.LessonDayId = lessonDay.Id;
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Lesson assigned successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning lesson");
            return StatusCode(500, new { message = "An error occurred while assigning the lesson." });
        }
    }

    [HttpDelete("unassign/{lessonId}")]
    public async Task<ActionResult> UnassignLesson(int lessonId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var lesson = await _dbContext.Lessons
                .Include(l => l.LessonPlan)
                .FirstOrDefaultAsync(l => l.Id == lessonId);

            if (lesson == null || lesson.LessonPlan?.UserId != userId)
                return NotFound(new { message = "Lesson not found." });

            var dayId = lesson.LessonDayId;
            lesson.LessonDayId = null;
            await _dbContext.SaveChangesAsync();

            if (dayId != null)
            {
                var day = await _dbContext.LessonDays
                    .Include(ld => ld.Lessons)
                    .FirstOrDefaultAsync(ld => ld.Id == dayId);

                if (day != null && day.Lessons.Count == 0)
                {
                    _dbContext.LessonDays.Remove(day);
                    await _dbContext.SaveChangesAsync();
                }
            }

            return Ok(new { message = "Lesson unassigned successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unassigning lesson");
            return StatusCode(500, new { message = "An error occurred while unassigning the lesson." });
        }
    }

    [HttpGet("date/{date}")]
    public async Task<ActionResult<LessonDayDto>> GetLessonDayByDate(DateTime date)
    {
        try
        {
            var userId = GetCurrentUserId();
            var searchDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
            var nextDay = searchDate.AddDays(1);

            var lessonDay = await _dbContext.LessonDays
                .Include(ld => ld.Lessons)
                    .ThenInclude(l => l.LessonPlan)
                .Where(ld => ld.Date >= searchDate && ld.Date < nextDay &&
                             ld.Lessons.Any(l => l.LessonPlan.UserId == userId))
                .Select(ld => new LessonDayDto
                {
                    Id = ld.Id,
                    Date = ld.Date,
                    Name = ld.Name,
                    ShortDescription = ld.ShortDescription,
                    Lessons = ld.Lessons
                        .Where(l => l.LessonPlan.UserId == userId)
                        .Select(l => new AssignedLessonDto
                        {
                            Id = l.Id,
                            LessonNumber = l.LessonNumber,
                            Name = l.Name,
                            ShortDescription = l.ShortDescription,
                            LessonPlanId = l.LessonPlanId,
                            LessonPlanName = l.LessonPlan.Name,
                            IsCompleted = l.IsCompleted
                        }).ToList()
                })
                .FirstOrDefaultAsync();

            return Ok(lessonDay);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving lesson day for date {Date}", date);
            return StatusCode(500, new { message = "An error occurred while retrieving the lesson day." });
        }
    }
}
