using Serilog;
using Serilog.Events;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

//koya commented
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "MyApp")
    .WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter())
    .WriteTo.File(
        formatter: new Serilog.Formatting.Json.JsonFormatter(),
        path: "logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .CreateLogger();

// Use Serilog for logging
builder.Host.UseSerilog();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLogging();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorchingg"
};

app.MapGet("api/v2", (ILogger<Program> logger) =>
{
    logger.LogInformation("Home Page Warning");
    var now = DateTime.UtcNow;
    return Results.Text(@$"
    <html>
    <head>
    <link rel='stylesheet' href='https://cdn.simplecss.org/simple-v1.css'>
    </head>
    <body>
    <h1>Weather Forecast Service Home Page </h1>
    <p>The time now in UTC is {now.ToUniversalTime().ToString()} </p>
    </body>
    </html>
    ", "text/html");
});//.Produces(200, contentType: "text/html");

app.MapGet("api/v2/continents", (ILogger<Program> logger) =>
{
    
    logger.LogInformation("continents page");
    var now = DateTime.UtcNow;
    return Results.Text(@$"
    <html>
    <head>
    <link rel='stylesheet' href='https://cdn.simplecss.org/simple-v1.css'>
    </head>
    <body>
    <h1> Continents Weather Forecast  </h1>
    <p>The time now in UTC is {now.ToUniversalTime().ToString()} </p>
    </body>
    </html>
    ", "text/html");
});//.Produces(200, contentType: "text/html");
    
app.MapGet("api/v2/countries", (ILogger<Program> logger) =>
{
    logger.LogInformation("countries page");
    var now = DateTime.UtcNow;
    return Results.Text(@$"
    <html>
    <head>
    <link rel='stylesheet' href='https://cdn.simplecss.org/simple-v1.css'>
    </head>
    <body>
    <h1> Countries Weatherforecast at {now.ToUniversalTime().ToString()}  </h1>
    <p>The time now in UTC is  </p>
    </body>
    </html>
    ", "text/html");
});//.Produces(200, contentType: "text/html");

app.MapGet("api/v2/error", (ILogger<Program> logger) =>
{
    logger.LogError("the page warning");
    var now = DateTime.UtcNow;
    return Results.Text(@$"
    <html>
    <head>
    <link rel='stylesheet' href='https://cdn.simplecss.org/simple-v1.css'>
    </head>
    <body>
    <h1>  Log at {now.ToUniversalTime().ToString()}  </h1>
    <p>The time now in UTC is  </p>
    </body>
    </html>
    ", "text/html");
});

app.MapGet("api/v2/warning", (ILogger<Program> logger) =>
{
    logger.LogWarning("the page");
    var now = DateTime.UtcNow;
    return Results.Text(@$"
    <html>
    <head>
    <link rel='stylesheet' href='https://cdn.simplecss.org/simple-v1.css'>
    </head>
    <body>
    <h1>  Log at {now.ToUniversalTime().ToString()}  </h1>
    <p>The time now in UTC is  </p>
    </body>
    </html>
    ", "text/html");
});


app.MapGet("/health", () => Results.Ok("Healthy"));

app.MapGet("api/v2/random", (ILogger<Program> logger) =>
{
    logger.LogInformation("random page");
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateTime.Now.AddDays(index),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    var random = new Random();

    return Results.Text(@$"
    <html>
    <head>
    <link rel='stylesheet' href='https://cdn.simplecss.org/simple-v1.css'>
    </head>
    <body>
    <h1> Random Forecast  </h1>
    <pre>  {forecast[random.Next(1, summaries.Length)]} <pre>                    
    </body>
    </html>
    ", "text/html");
})
.WithName("GetWeatherForecast");

app.MapGet("api/v2/file", (string? key, string? type, ILogger<Program> logger) =>
{
    if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(type))
    {
        logger.LogInformation("No query params passed. Showing default page.");

        var html = @"
            <html>
            <head>
              <link rel='stylesheet' href='https://cdn.simplecss.org/simple-v1.css'>
            </head>
            <body>
              <h1> File Viewer </h1>
              <p>Use query parameters <code>?key=&lt;ENV_KEY&gt;&type=json|image</code></p>
              <p>Example: <code>/api/v2/file?key=MY_JSON_FILE&type=json</code></p>
            </body>
            </html>";

        return Results.Text(html, "text/html");
    }

    var filePath = Environment.GetEnvironmentVariable(key);

    if (string.IsNullOrEmpty(filePath))
    {
        logger.LogWarning("No environment variable found for key '{Key}'", key);
        return Results.BadRequest($"No file path found for environment variable '{key}'");
    }

    if (!File.Exists(filePath))
    {
        logger.LogWarning("File not found at path: {FilePath}", filePath);
        return Results.BadRequest($"No file found at path '{filePath}'");
    }

    logger.LogInformation("Loading file from path: {FilePath}", filePath);

    if (type.Equals("json", StringComparison.OrdinalIgnoreCase))
    {
        try
        {
            var jsonContent = File.ReadAllText(filePath);

            using var doc = JsonDocument.Parse(jsonContent);
            var prettyJson = JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions { WriteIndented = true });

            var html = @$"
                <html>
                <head>
                  <link rel='stylesheet' href='https://cdn.simplecss.org/simple-v1.css'>
                </head>
                <body>
                  <h1> JSON File from {key} </h1>
                  <pre>{System.Net.WebUtility.HtmlEncode(prettyJson)}</pre>
                </body>
                </html>";

            logger.LogInformation("Successfully served JSON file: {FilePath}", filePath);
            return Results.Text(html, "text/html");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Invalid JSON in file {FilePath}", filePath);
            return Results.BadRequest("Invalid JSON format in file.");
        }
    }
    else if (type.Equals("image", StringComparison.OrdinalIgnoreCase))
    {
        try
        {
            var bytes = File.ReadAllBytes(filePath);
            var base64 = Convert.ToBase64String(bytes);

            var html = @$"
                <html>
                <head>
                  <link rel='stylesheet' href='https://cdn.simplecss.org/simple-v1.css'>
                </head>
                <body>
                  <h1> Image from {key} </h1>
                  <img src='data:image/png;base64,{base64}' alt='Image from file {filePath}' style='max-width:600px;' />
                </body>
                </html>";

            logger.LogInformation("Successfully served image file: {FilePath}", filePath);
            return Results.Text(html, "text/html");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load image from {FilePath}", filePath);
            return Results.BadRequest("Could not load image.");
        }
    }

    logger.LogWarning("Unsupported file type '{Type}' requested for file {FilePath}", type, filePath);
    return Results.BadRequest("Unsupported file type. Use 'json' or 'image'.");
});


app.Run("http://0.0.0.0:8080");

record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
