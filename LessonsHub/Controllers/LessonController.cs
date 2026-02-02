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
        if (!lesson.Videos.Any() && !lesson.Books.Any() && !lesson.Documentation.Any())
        {
            _logger.LogInformation("Generating resources for Lesson {Id}...", id);

            try
            {
                var resourcesRequest = new AiLessonResourcesRequest
                {
                    Topic = planTopic,
                    LessonName = lesson.Name,
                    LessonDescription = lesson.ShortDescription ?? ""
                };

                var resourcesResponse = await _aiApiClient.GenerateLessonResourcesAsync(resourcesRequest);

                if (resourcesResponse != null)
                {
                    // Add videos
                    foreach (var video in resourcesResponse.Videos)
                    {
                        lesson.Videos.Add(new Video
                        {
                            Title = video.Title,
                            Channel = video.Channel,
                            Description = video.Description,
                            Url = video.Url,
                            LessonId = lesson.Id
                        });
                    }

                    // Add books
                    foreach (var book in resourcesResponse.Books)
                    {
                        lesson.Books.Add(new Book
                        {
                            Author = book.Author,
                            BookName = book.BookName,
                            ChapterNumber = book.ChapterNumber,
                            ChapterName = book.ChapterName,
                            Description = book.Description,
                            Url = book.Url,
                            LessonId = lesson.Id
                        });
                    }

                    // Add documentation
                    foreach (var doc in resourcesResponse.Documentation)
                    {
                        lesson.Documentation.Add(new Documentation
                        {
                            Name = doc.Name,
                            Section = doc.Section,
                            Description = doc.Description,
                            Url = doc.Url,
                            LessonId = lesson.Id
                        });
                    }

                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("Resources generated and saved for Lesson {Id}", id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating resources for Lesson {Id}", id);
            }
        }

        return Ok(lesson);
    }
}
