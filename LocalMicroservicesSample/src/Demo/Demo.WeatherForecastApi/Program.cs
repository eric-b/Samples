using Demo.WeatherForecastApi.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Demo.Infrastructure.OpenTelemetry.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddOpenTelemetry();

builder.Services.AddDbContext<SummaryDbContext>(options=> options.UseSqlServer(builder.Configuration.GetValue<string>("SqlDatabase:ConnectionString")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapGet("/api/v1/weatherforecast", (SummaryDbContext db) =>
{
    DbInitializer.Initialize(db);
    var forecast = Enumerable.Range(1, 5).Select(index =>
    {
        var summaries = db.Summaries.ToArray();
        return new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)].Label
        );
    }).ToArray();
    return forecast;
});

app.UseSwagger();
app.UseSwaggerUI();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}