using Google.Apis.Auth;
using LessonsHub.Data;
using LessonsHub.Entities;
using LessonsHub.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LessonsHub.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly LessonsHubDbContext _dbContext;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        LessonsHubDbContext dbContext,
        ITokenService tokenService,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("google")]
    public async Task<ActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        try
        {
            var clientId = _configuration["GoogleAuth:ClientId"];
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { clientId }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.GoogleId == payload.Subject);

            if (user == null)
            {
                user = new User
                {
                    GoogleId = payload.Subject,
                    Email = payload.Email,
                    Name = payload.Name,
                    PictureUrl = payload.Picture,
                    CreatedAt = DateTime.UtcNow
                };
                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("New user registered: {Email}", user.Email);
            }

            var token = _tokenService.CreateToken(user);
            return Ok(new
            {
                token,
                user = new { user.Id, user.Email, user.Name, user.PictureUrl }
            });
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogWarning(ex, "Invalid Google ID token");
            return Unauthorized(new { message = "Invalid Google token." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Google login");
            return StatusCode(500, new { message = "Authentication failed." });
        }
    }
}

public class GoogleLoginRequest
{
    public string IdToken { get; set; } = string.Empty;
}
