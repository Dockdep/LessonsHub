using LessonsHub.Infrastructure.Data;
using LessonsHub.Domain.Entities;
using LessonsHub.Application.Interfaces;
using LessonsHub.Application.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LessonsHub.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class LessonController : ControllerBase
{
    private readonly LessonsHubDbContext _dbContext;
    private readonly ILessonsAiApiClient _aiApiClient;
    private readonly ILogger<LessonController> _logger;

    public LessonController(
        LessonsHubDbContext dbContext,
        ILessonsAiApiClient aiApiClient,
        ILogger<LessonController> logger)
    {
        _dbContext = dbContext;
        _aiApiClient = aiApiClient;
        _logger = logger;
    }

    private int GetCurrentUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("{id}")]
    public async Task<ActionResult<Lesson>> GetLesson(int id)
    {
        var userId = GetCurrentUserId();
        var lesson = await _dbContext.Lessons
            .Include(l => l.Exercises)
                .ThenInclude(e => e.Answers)
            .Include(l => l.LessonPlan)
            .Include(l => l.Videos)
            .Include(l => l.Books)
            .Include(l => l.Documentation)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lesson == null || lesson.LessonPlan?.UserId != userId) return NotFound();

        var planName = lesson.LessonPlan?.Name ?? "General Course";
        var planTopic = lesson.LessonPlan?.Topic ?? planName;
        var planDescription = lesson.LessonPlan?.Description ?? "";

        if (string.IsNullOrWhiteSpace(lesson.Content))
        {
            _logger.LogInformation("Generating content for Lesson {Id}...", id);

            try
            {
                var contentRequest = new AiLessonContentRequest
                {
                    PlanName = planName,
                    Topic = planTopic,
                    LessonType = lesson.LessonType,
                    LessonTopic = lesson.LessonTopic,
                    KeyPoints = lesson.KeyPoints ?? new(),
                    PlanDescription = planDescription,
                    LessonNumber = lesson.LessonNumber,
                    LessonName = lesson.Name,
                    LessonDescription = lesson.ShortDescription ?? "",
                    Language = lesson.LessonPlan?.NativeLanguage
                };

                var contentResponse = await _aiApiClient.GenerateLessonContentAsync(contentRequest);

                if (contentResponse != null && !string.IsNullOrWhiteSpace(contentResponse.Content))
                {
                    lesson.Content = contentResponse.Content;
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("Content generated and saved for Lesson {Id}", id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating content for Lesson {Id}", id);
            }
        }

        return Ok(lesson);
    }

    [HttpPost("{id}/generate-exercise")]
    public async Task<ActionResult<Exercise>> GenerateExercise(int id, [FromQuery] string difficulty = "medium", [FromQuery] string? comment = null)
    {
        var userId = GetCurrentUserId();
        var lesson = await _dbContext.Lessons
            .Include(l => l.LessonPlan)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lesson == null || lesson.LessonPlan?.UserId != userId) return NotFound();

        if (string.IsNullOrWhiteSpace(lesson.Content))
            return BadRequest(new { message = "Lesson content must be generated first." });

        try
        {
            var planName = lesson.LessonPlan?.Name ?? "General Course";
            var planTopic = lesson.LessonPlan?.Topic ?? planName;
            var planDescription = lesson.LessonPlan?.Description ?? "";

            var exerciseRequest = new AiLessonExerciseRequest
            {
                PlanName = planName,
                PlanTopic = planTopic,
                PlanDescription = planDescription,
                LessonType = lesson.LessonType,
                LessonTopic = lesson.LessonTopic,
                LessonNumber = lesson.LessonNumber,
                LessonName = lesson.Name,
                LessonDescription = lesson.ShortDescription ?? "",
                LessonContent = lesson.Content,
                KeyPoints = lesson.KeyPoints ?? new(),
                Difficulty = difficulty,
                Comment = comment,
                NativeLanguage = lesson.LessonPlan?.NativeLanguage
            };

            var exerciseResponse = await _aiApiClient.GenerateLessonExerciseAsync(exerciseRequest);

            if (exerciseResponse == null || string.IsNullOrWhiteSpace(exerciseResponse.Exercise))
                return StatusCode(500, new { message = "Failed to generate exercise from AI API." });

            var exercise = new Exercise
            {
                ExerciseText = exerciseResponse.Exercise,
                Difficulty = difficulty,
                LessonId = lesson.Id
            };

            _dbContext.Exercises.Add(exercise);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Exercise generated and saved for Lesson {Id}", id);
            return Ok(exercise);
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "Timeout generating exercise for Lesson {Id}", id);
            return StatusCode(504, new { message = "The AI service is taking too long. Please try again." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating exercise for Lesson {Id}", id);
            return StatusCode(500, new { message = "An error occurred while generating the exercise.", error = ex.Message });
        }
    }

    [HttpPost("{id}/retry-exercise")]
    public async Task<ActionResult<Exercise>> RetryExercise(int id, [FromQuery] string difficulty = "medium", [FromQuery] string? comment = null, [FromQuery] string review = "")
    {
        var userId = GetCurrentUserId();
        var lesson = await _dbContext.Lessons
            .Include(l => l.LessonPlan)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lesson == null || lesson.LessonPlan?.UserId != userId) return NotFound();

        if (string.IsNullOrWhiteSpace(lesson.Content))
            return BadRequest(new { message = "Lesson content must be generated first." });

        if (string.IsNullOrWhiteSpace(review))
            return BadRequest(new { message = "Review text is required for retry." });

        try
        {
            var planName = lesson.LessonPlan?.Name ?? "General Course";
            var planTopic = lesson.LessonPlan?.Topic ?? planName;
            var planDescription = lesson.LessonPlan?.Description ?? "";

            var retryRequest = new AiExerciseRetryRequest
            {
                PlanName = planName,
                PlanTopic = planTopic,
                PlanDescription = planDescription,
                LessonType = lesson.LessonType,
                LessonTopic = lesson.LessonTopic,
                LessonNumber = lesson.LessonNumber,
                LessonName = lesson.Name,
                LessonDescription = lesson.ShortDescription ?? "",
                LessonContent = lesson.Content,
                KeyPoints = lesson.KeyPoints ?? new(),
                Difficulty = difficulty,
                Review = review,
                Comment = comment,
                NativeLanguage = lesson.LessonPlan?.NativeLanguage
            };

            var exerciseResponse = await _aiApiClient.RetryLessonExerciseAsync(retryRequest);

            if (exerciseResponse == null || string.IsNullOrWhiteSpace(exerciseResponse.Exercise))
                return StatusCode(500, new { message = "Failed to generate exercise from AI API." });

            var exercise = new Exercise
            {
                ExerciseText = exerciseResponse.Exercise,
                Difficulty = difficulty,
                LessonId = lesson.Id
            };

            _dbContext.Exercises.Add(exercise);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Retry exercise generated and saved for Lesson {Id}", id);
            return Ok(exercise);
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "Timeout retrying exercise for Lesson {Id}", id);
            return StatusCode(504, new { message = "The AI service is taking too long. Please try again." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying exercise for Lesson {Id}", id);
            return StatusCode(500, new { message = "An error occurred while generating the exercise.", error = ex.Message });
        }
    }

    [HttpPost("exercise/{exerciseId}/check")]
    public async Task<ActionResult<ExerciseAnswer>> CheckExerciseReview(int exerciseId, [FromBody] string answer)
    {
        var userId = GetCurrentUserId();
        var exercise = await _dbContext.Exercises
            .Include(e => e.Lesson).ThenInclude(l => l.LessonPlan)
            .FirstOrDefaultAsync(e => e.Id == exerciseId);

        if (exercise == null || exercise.Lesson.LessonPlan?.UserId != userId) return NotFound();

        if (string.IsNullOrWhiteSpace(answer))
            return BadRequest(new { message = "Answer cannot be empty." });

        try
        {
            var reviewRequest = new AiExerciseReviewRequest
            {
                LessonType = exercise.Lesson.LessonType,
                LessonContent = exercise.Lesson.Content,
                ExerciseContent = exercise.ExerciseText,
                Difficulty = exercise.Difficulty,
                Answer = answer,
                Language = exercise.Lesson.LessonPlan?.NativeLanguage
            };

            var reviewResponse = await _aiApiClient.CheckExerciseReviewAsync(reviewRequest);

            if (reviewResponse == null)
                return StatusCode(500, new { message = "Failed to get review from AI API." });

            var exerciseAnswer = new ExerciseAnswer
            {
                UserResponse = answer,
                SubmittedAt = DateTime.UtcNow,
                AccuracyLevel = reviewResponse.AccuracyLevel,
                ReviewText = reviewResponse.ExamReview,
                ExerciseId = exerciseId
            };

            _dbContext.ExerciseAnswers.Add(exerciseAnswer);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Exercise review saved for Exercise {ExerciseId}", exerciseId);
            return Ok(exerciseAnswer);
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "Timeout checking exercise review for Exercise {ExerciseId}", exerciseId);
            return StatusCode(504, new { message = "The AI service is taking too long. Please try again." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking exercise review for Exercise {ExerciseId}", exerciseId);
            return StatusCode(500, new { message = "An error occurred while checking the exercise.", error = ex.Message });
        }
    }

    [HttpGet("{id}/siblings")]
    public async Task<ActionResult> GetSiblingLessonIds(int id)
    {
        var userId = GetCurrentUserId();
        var current = await _dbContext.Lessons
            .AsNoTracking()
            .Include(l => l.LessonPlan)
            .Select(l => new { l.Id, l.LessonPlanId, l.LessonNumber, UserId = l.LessonPlan.UserId })
            .FirstOrDefaultAsync(l => l.Id == id);

        if (current == null || current.UserId != userId) return NotFound();

        var siblings = _dbContext.Lessons.Where(l => l.LessonPlanId == current.LessonPlanId);

        var prevId = await siblings
            .Where(l => l.LessonNumber < current.LessonNumber)
            .OrderByDescending(l => l.LessonNumber)
            .Select(l => (int?)l.Id)
            .FirstOrDefaultAsync();

        var nextId = await siblings
            .Where(l => l.LessonNumber > current.LessonNumber)
            .OrderBy(l => l.LessonNumber)
            .Select(l => (int?)l.Id)
            .FirstOrDefaultAsync();

        return Ok(new { prevLessonId = prevId, nextLessonId = nextId });
    }

    [HttpPatch("{id}/complete")]
    public async Task<ActionResult<Lesson>> ToggleLessonComplete(int id)
    {
        var userId = GetCurrentUserId();
        var lesson = await _dbContext.Lessons
            .Include(l => l.Exercises)
                .ThenInclude(e => e.Answers)
            .Include(l => l.LessonPlan)
            .Include(l => l.Videos)
            .Include(l => l.Books)
            .Include(l => l.Documentation)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lesson == null || lesson.LessonPlan?.UserId != userId) return NotFound();

        lesson.IsCompleted = !lesson.IsCompleted;
        lesson.CompletedAt = lesson.IsCompleted ? DateTime.UtcNow : null;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Lesson {Id} marked as {Status}", id, lesson.IsCompleted ? "completed" : "incomplete");
        return Ok(lesson);
    }
}
