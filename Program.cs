using Serilog;
using Serilog.Events;

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
    logger.LogInformation("Home Page");
    logger.LogError("Home Page error");
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
    logger.LogError("errors page");
    var now = DateTime.UtcNow;
    return Results.Text(@$"
    <html>
    <head>
    <link rel='stylesheet' href='https://cdn.simplecss.org/simple-v1.css'>
    </head>
    <body>
    <h1> Error Log at {now.ToUniversalTime().ToString()}  </h1>
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

app.Run("http://0.0.0.0:8080");

record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
