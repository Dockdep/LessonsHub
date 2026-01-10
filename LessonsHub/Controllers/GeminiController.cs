using Microsoft.AspNetCore.Mvc;
using LessonsHub.Models;
using LessonsHub.Services;

namespace LessonsHub.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GeminiController : ControllerBase
{
    private readonly IGeminiService _geminiService;
    private readonly ILogger<GeminiController> _logger;

    public GeminiController(IGeminiService geminiService, ILogger<GeminiController> logger)
    {
        _geminiService = geminiService;
        _logger = logger;
    }

    /// <summary>
    /// Send messages to Gemini AI and receive a response
    /// </summary>
    /// <param name="request">The request containing messages to send to Gemini</param>
    /// <returns>The response from Gemini AI</returns>
    [HttpPost("chat")]
    [ProducesResponseType(typeof(GeminiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Chat([FromBody] GeminiRequest request)
    {
        try
        {
            if (request?.Messages == null || !request.Messages.Any())
            {
                return BadRequest("Messages cannot be empty");
            }

            _logger.LogInformation("Processing Gemini chat request with {MessageCount} messages", request.Messages.Count);

            var response = await _geminiService.SendMessageAsync(request);

            _logger.LogInformation("Successfully received response from Gemini. Tokens used: {TokensUsed}", response.TokensUsed);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Gemini chat request");
            return StatusCode(500, new { error = "An error occurred while processing your request", details = ex.Message });
        }
    }
}
