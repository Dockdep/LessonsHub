using LessonsHub.Data;
using LessonsHub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LessonsHub.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LessonDayController : ControllerBase
{
    private readonly LessonsHubDbContext _dbContext;
    private readonly ILogger<LessonDayController> _logger;

    public LessonDayController(LessonsHubDbContext dbContext, ILogger<LessonDayController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpGet("plans")]
    public async Task<ActionResult<List<LessonPlanSummaryDto>>> GetLessonPlans()
    {
        try
        {
            var plans = await _dbContext.LessonPlans
                .Include(lp => lp.Lessons)
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
            var lessonPlan = await _dbContext.LessonPlans
                .Include(lp => lp.Lessons)
                .FirstOrDefaultAsync(lp => lp.Id == lessonPlanId);

            if (lessonPlan == null)
            {
                return NotFound(new { message = "Lesson plan not found." });
            }

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
            var startDate = DateTime.SpecifyKind(new DateTime(year, month, 1), DateTimeKind.Utc);
            var endDate = startDate.AddMonths(1);

            var lessonDays = await _dbContext.LessonDays
                .Include(ld => ld.Lessons)
                    .ThenInclude(l => l.LessonPlan)
                .Where(ld => ld.Date >= startDate && ld.Date < endDate)
                .OrderBy(ld => ld.Date)
                .Select(ld => new LessonDayDto
                {
                    Id = ld.Id,
                    Date = ld.Date,
                    Name = ld.Name,
                    ShortDescription = ld.ShortDescription,
                    Lessons = ld.Lessons.Select(l => new AssignedLessonDto
                    {
                        Id = l.Id,
                        LessonNumber = l.LessonNumber,
                        Name = l.Name,
                        ShortDescription = l.ShortDescription,
                        LessonPlanId = l.LessonPlanId,
                        LessonPlanName = l.LessonPlan.Name
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
            var lesson = await _dbContext.Lessons.FindAsync(request.LessonId);
            if (lesson == null)
            {
                return NotFound(new { message = "Lesson not found." });
            }

            if (!DateTime.TryParse(request.Date, out var date))
            {
                return BadRequest(new { message = "Invalid date format." });
            }

            // Ensure DateTime is in UTC for PostgreSQL
            var utcDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);

            // Check if a lesson day exists for this date
            var lessonDay = await _dbContext.LessonDays
                .FirstOrDefaultAsync(ld => ld.Date.Date == utcDate.Date);

            if (lessonDay == null)
            {
                // Create new lesson day
                lessonDay = new LessonDay
                {
                    Date = utcDate,
                    Name = request.DayName,
                    ShortDescription = request.DayDescription
                };
                _dbContext.LessonDays.Add(lessonDay);
                await _dbContext.SaveChangesAsync();
            }

            // Assign lesson to the day
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
            var lesson = await _dbContext.Lessons.FindAsync(lessonId);
            if (lesson == null)
            {
                return NotFound(new { message = "Lesson not found." });
            }

            lesson.LessonDayId = null;
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Lesson unassigned successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unassigning lesson");
            return StatusCode(500, new { message = "An error occurred while unassigning the lesson." });
        }
    }
}
