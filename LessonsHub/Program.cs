using LessonsHub.Data;
using LessonsHub.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext with PostgreSQL
builder.Services.AddDbContext<LessonsHubDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container
builder.Services.AddControllers()
	.AddNewtonsoftJson(options =>
	{
		// Keep your existing CamelCase setting
		options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();

		// ADD THIS LINE to fix the error:
		options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
	});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Add HttpClient for GeminiService
builder.Services.AddHttpClient<IGeminiService, GeminiService>();

// Add GeminiJsonService for JSON parsing
builder.Services.AddScoped<IGeminiJsonService, GeminiJsonService>();

// Add PromptService
builder.Services.AddSingleton<IPromptService, PromptService>();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "LessonsHub - Gemini API",
        Version = "v1",
        Description = "API for communicating with Google Gemini AI"
    });
});

// Add SPA static files support
builder.Services.AddSpaStaticFiles(configuration =>
{
    configuration.RootPath = "ClientApp/dist/client-app/browser";
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Gemini API v1");
        options.RoutePrefix = "swagger"; // Move Swagger to /swagger to avoid conflict with Angular
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular");

// Serve static files from ClientApp
app.UseStaticFiles();
app.UseSpaStaticFiles();

app.UseRouting();
app.UseAuthorization();

// IMPORTANT: Map controllers BEFORE UseSpa
app.MapControllers();

// Configure SPA - this should be LAST as it's a catch-all
app.MapWhen(context => !context.Request.Path.StartsWithSegments("/api") &&
                       !context.Request.Path.StartsWithSegments("/swagger"),
    appBuilder =>
    {
        appBuilder.UseSpa(spa =>
        {
            spa.Options.SourcePath = "ClientApp";

            if (app.Environment.IsDevelopment())
            {
                spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
            }
        });
    });

app.Run();
