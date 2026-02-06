using LessonsHub.Data;
using LessonsHub.Entities;
using LessonsHub.Interfaces;
using LessonsHub.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LessonsHub.Controllers;

[Route("api/[controller]")]
[ApiController]
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

    [HttpGet("{id}")]
    public async Task<ActionResult<Lesson>> GetLesson(int id)
    {
        var lesson = await _dbContext.Lessons
            .Include(l => l.Exercises)
                .ThenInclude(e => e.Answers)
            .Include(l => l.LessonPlan)
            .Include(l => l.Videos)
            .Include(l => l.Books)
            .Include(l => l.Documentation)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lesson == null) return NotFound();

        var planName = lesson.LessonPlan?.Name ?? "General Course";
        var planTopic = lesson.LessonPlan?.Topic ?? planName;
        var planDescription = lesson.LessonPlan?.Description ?? "";

        // Generate content if missing
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
                    LessonDescription = lesson.ShortDescription ?? ""
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

        // Generate resources if missing
        //if (!lesson.Videos.Any() && !lesson.Books.Any() && !lesson.Documentation.Any())
        //{
        //    _logger.LogInformation("Generating resources for Lesson {Id}...", id);

        //    try
        //    {
        //        var resourcesRequest = new AiLessonResourcesRequest
        //        {
        //            LessonType = lesson.LessonType,
        //            Topic = planTopic,
        //            LessonName = lesson.Name,
        //            LessonTopic = lesson.LessonTopic,
        //            LessonDescription = lesson.ShortDescription ?? ""
        //        };

        //        var resourcesResponse = await _aiApiClient.GenerateLessonResourcesAsync(resourcesRequest);

                //        if (resourcesResponse != null)
                //        {
                //            // Add videos
                //            foreach (var video in resourcesResponse.Videos)
                //            {
                //                lesson.Videos.Add(new Video
                //                {
                //                    Title = video.Title,
                //                    Channel = video.Channel,
                //                    Description = video.Description,
                //                    Url = video.Url,
                //                    LessonId = lesson.Id
                //                });
                //            }

                //            // Add books
                //            foreach (var book in resourcesResponse.Books)
                //            {
                //                lesson.Books.Add(new Book
                //                {
                //                    Author = book.Author,
                //                    BookName = book.BookName,
                //                    ChapterNumber = book.ChapterNumber,
                //                    ChapterName = book.ChapterName,
                //                    Description = book.Description,
                //                    LessonId = lesson.Id
                //                });
                //            }

                //            // Add documentation
                //            foreach (var doc in resourcesResponse.Documentation)
                //            {
                //                lesson.Documentation.Add(new Documentation
                //                {
                //                    Name = doc.Name,
                //                    Section = doc.Section,
                //                    Description = doc.Description,
                //                    Url = doc.Url,
                //                    LessonId = lesson.Id
                //                });
                //            }

                //            await _dbContext.SaveChangesAsync();
                //            _logger.LogInformation("Resources generated and saved for Lesson {Id}", id);
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        _logger.LogError(ex, "Error generating resources for Lesson {Id}", id);
                //    }
                //}

                return Ok(lesson);
    }

    [HttpPost("{id}/generate-exercise")]
    public async Task<ActionResult<Exercise>> GenerateExercise(int id, [FromQuery] string difficulty = "medium", [FromQuery] string? comment = null)
    {
        var lesson = await _dbContext.Lessons
            .Include(l => l.LessonPlan)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lesson == null) return NotFound();

        if (string.IsNullOrWhiteSpace(lesson.Content))
        {
            return BadRequest(new { message = "Lesson content must be generated first." });
        }

        try
        {
            var exerciseRequest = new AiLessonExerciseRequest
            {
                LessonType = lesson.LessonType,
                LessonTopic = lesson.LessonTopic,
                LessonNumber = lesson.LessonNumber,
                LessonName = lesson.Name,
                LessonDescription = lesson.ShortDescription ?? "",
                LessonContent = lesson.Content,
                Difficulty = difficulty,
                Comment = comment,
                NativeLanguage = lesson.LessonPlan?.NativeLanguage
            };

            var exerciseResponse = await _aiApiClient.GenerateLessonExerciseAsync(exerciseRequest);

            if (exerciseResponse == null || string.IsNullOrWhiteSpace(exerciseResponse.Exercise))
            {
                return StatusCode(500, new { message = "Failed to generate exercise from AI API." });
            }

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

    [HttpPost("exercise/{exerciseId}/check")]
    public async Task<ActionResult<ExerciseAnswer>> CheckExerciseReview(int exerciseId, [FromBody] string answer)
    {
        var exercise = await _dbContext.Exercises
            .Include(e => e.Lesson)
            .FirstOrDefaultAsync(e => e.Id == exerciseId);

        if (exercise == null) return NotFound();

        if (string.IsNullOrWhiteSpace(answer))
        {
            return BadRequest(new { message = "Answer cannot be empty." });
        }

        try
        {
            var reviewRequest = new AiExerciseReviewRequest
            {
                LessonType = exercise.Lesson.LessonType,
                LessonContent = exercise.Lesson.Content,
                ExerciseContent = exercise.ExerciseText,
                Difficulty = exercise.Difficulty,
                Answer = answer
            };

            var reviewResponse = await _aiApiClient.CheckExerciseReviewAsync(reviewRequest);

            if (reviewResponse == null)
            {
                return StatusCode(500, new { message = "Failed to get review from AI API." });
            }

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
}
