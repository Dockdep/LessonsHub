# Lessons Plan App - Angular Integration Guide

This document explains how the Angular app is integrated with the ASP.NET Core backend.

## Project Structure

```
LessonsHub/
├── Controllers/
│   └── GeminiController.cs          # API controller with POST endpoint
├── Services/
│   ├── IGeminiService.cs            # Service interface
│   └── GeminiService.cs             # Gemini communication logic
├── Models/
│   ├── GeminiRequest.cs             # Request model
│   └── GeminiResponse.cs            # Response model
├── ClientApp/                        # Angular Application
│   ├── src/
│   │   ├── app/
│   │   │   ├── chat/                # Chat component
│   │   │   ├── models/              # TypeScript models
│   │   │   ├── services/            # Angular services
│   │   │   └── app.ts               # Main app component
│   │   └── ...
│   ├── angular.json                 # Angular configuration
│   ├── proxy.conf.json              # Development proxy config
│   └── package.json                 # Node dependencies
├── Program.cs                        # Application entry point with SPA config
└── appsettings.json                 # Configuration file
```

## How It Works

### Development Mode

In development, the ASP.NET Core app and Angular app run separately:

1. **ASP.NET Core** runs on `https://localhost:7121` (or `http://localhost:5191`)
   - Serves the API endpoints (`/api/*`)
   - Serves Swagger UI (`/swagger`)
   - Proxies unknown requests to Angular dev server

2. **Angular** runs on `http://localhost:4200` via `ng serve`
   - Proxies API requests back to ASP.NET Core
   - Enables hot module replacement for fast development

### Production Mode

In production, the Angular app is built and served by ASP.NET Core:

1. Angular app is built to static files in `ClientApp/dist/`
2. ASP.NET Core serves these static files
3. All requests go through ASP.NET Core
4. Angular handles client-side routing

## Running the Application

### Option 1: Development Mode (Recommended for Development)

Run both servers separately for best development experience:

**Terminal 1 - ASP.NET Core Backend:**
```bash
cd LessonsHub
dotnet run
```

**Terminal 2 - Angular Frontend:**
```bash
cd LessonsHub/ClientApp
ng serve
```

Then open your browser to:
- Angular App: `http://localhost:4200`
- Swagger API: `https://localhost:7121/swagger`

### Option 2: Integrated Mode

Run only the ASP.NET Core app, which will proxy to Angular:

```bash
cd LessonsHub/ClientApp
ng serve

# In another terminal
cd LessonsHub
dotnet run
```

Then open: `https://localhost:7121`

### Option 3: Production Build

Build Angular and run everything through ASP.NET Core:

```bash
cd LessonsHub/ClientApp
npm run build

cd ..
dotnet run
```

Then open: `https://localhost:7121`

## Configuration

### API Endpoints

The Angular app communicates with the following endpoints:

- **POST** `/api/gemini/chat` - Send messages to Gemini AI
  - Request: `{ messages: [{ role: string, content: string }] }`
  - Response: `{ content: string, model: string, tokensUsed: number }`

### Proxy Configuration

[proxy.conf.json](ClientApp/proxy.conf.json) configures the Angular dev server to forward API requests to the ASP.NET Core backend:

```json
{
  "/api": {
    "target": "https://localhost:7121",
    "secure": false,
    "changeOrigin": true
  }
}
```

### Environment Settings

Update your Gemini API key in:
- `appsettings.json` - Production configuration
- `appsettings.Development.json` - Development configuration

```json
{
  "Gemini": {
    "ApiKey": "YOUR_GEMINI_API_KEY_HERE",
    "Model": "gemini-3-flash-preview"
  }
}
```

## Publishing

When you publish the application, the Angular app is automatically built:

```bash
cd LessonsHub
dotnet publish -c Release
```

The `LessonsHub.csproj` file contains a custom target that:
1. Runs `npm install` in the ClientApp folder
2. Runs `npm run build` to build the Angular app
3. Includes the built files in the publish output

## Key Integration Points

### 1. Program.cs Configuration

[Program.cs](Program.cs:1-65) configures the SPA integration:

```csharp
// Add SPA static files support
builder.Services.AddSpaStaticFiles(configuration =>
{
    configuration.RootPath = "ClientApp/dist/client-app/browser";
});

// Configure SPA
app.UseSpa(spa =>
{
    spa.Options.SourcePath = "ClientApp";

    if (app.Environment.IsDevelopment())
    {
        spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
    }
});
```

### 2. Angular HTTP Client

The [GeminiService](ClientApp/src/app/services/gemini.service.ts) uses Angular's HttpClient:

```typescript
sendMessage(request: GeminiRequest): Observable<GeminiResponse> {
  return this.http.post<GeminiResponse>('/api/gemini/chat', request);
}
```

### 3. CORS

CORS is not needed because in development, the proxy handles cross-origin requests, and in production, everything is served from the same origin.

## Troubleshooting

### Angular CLI Not Found

Install Angular CLI globally:
```bash
npm install -g @angular/cli
```

### Port Already in Use

If port 4200 or 7121 is in use, you can:
- Change the Angular port: `ng serve --port 4201`
- Change the ASP.NET Core port in [launchSettings.json](Properties/launchSettings.json)
- Update the proxy configuration accordingly

### API Requests Failing

1. Ensure the ASP.NET Core backend is running
2. Check the proxy configuration in [proxy.conf.json](ClientApp/proxy.conf.json)
3. Verify the API key is configured in appsettings.json
4. Check the browser console and network tab for errors

### Build Errors

If you encounter build errors:
```bash
cd ClientApp
rm -rf node_modules package-lock.json
npm install
ng build
```

## Available NPM Commands

In the `ClientApp` folder:

- `npm start` or `ng serve` - Start development server
- `npm run build` - Build for production
- `npm test` - Run unit tests
- `ng generate component <name>` - Generate new component

## Architecture Benefits

1. **Separation of Concerns**: Frontend and backend are clearly separated
2. **Hot Reload**: Angular dev server provides instant feedback during development
3. **Type Safety**: TypeScript models match C# models
4. **Production Ready**: Single deployment artifact with optimized bundles
5. **API Documentation**: Swagger available for API testing

## Next Steps

- Add authentication/authorization
- Implement conversation history persistence
- Add error boundary and retry logic
- Add unit and integration tests
- Implement CI/CD pipeline
- Add environment-specific configurations
