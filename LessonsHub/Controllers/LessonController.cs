using LessonsHub.Data;
using LessonsHub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LessonsHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LessonController : ControllerBase
    {
        private readonly LessonsHubDbContext _dbContext;

        public LessonController(LessonsHubDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Lesson>> GetLesson(int id)
        {
            var lesson = await _dbContext.Lessons
                .Include(l => l.Exercises)
                .Include(l => l.LessonPlan) // Include parent plan for context
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null)
            {
                return NotFound();
            }

            return Ok(lesson);
        }
    }
}