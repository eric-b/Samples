using AspNetCoreWebApp.Utility;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Otel = AspNetCoreWebApp.Tracing.OpenTelemetry;

namespace AspNetCoreWebApp.Components
{
    /// <summary>
    /// Fake weather forecast service
    /// </summary>
    public sealed class WeatherForecastService : IDisposable
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly Timer _timer;
        private readonly ILogger<WeatherForecastService> _logger;

        private readonly SemaphoreSlim _forecastSync;
        private readonly List<Model.WeatherForecast> _forecast;

        public WeatherForecastService(ILogger<WeatherForecastService> logger)
        {
            _timer = NonCapturingTimer.Create(TimerCallback, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            _logger = logger;
            _forecastSync = new SemaphoreSlim(initialCount: 1, maxCount: 1);
            _forecast = new List<Model.WeatherForecast>();
        }

        public void Dispose()
        {
            _timer.Dispose();
        }

        private async Task<Model.WeatherForecast[]> RequestForecast()
        {
            using (Otel.ActivitySource.StartActivity("request_actual_forecast"))
            {
                // Simulates request to another weather forecast service...
                _logger.LogInformation("Requesting forecast (backchannel)...");

                var rng = new Random();
                await Task.Delay(millisecondsDelay: rng.Next(500, 3000));
                return Enumerable.Range(1, 5).Select(index => new Model.WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = rng.Next(-20, 55),
                    Summary = Summaries[rng.Next(Summaries.Length)]
                })
                .ToArray();
            }
        }

        public async Task<Model.WeatherForecast[]> GetWeatherForecast()
        {
            using (Otel.ActivitySource.StartActivity("get_forecast_from_service"))
            {
                _logger.LogInformation("Get Weather called");

                await _forecastSync.WaitAsync();
                try
                {
                    if (_forecast.Count != 0)
                        return _forecast.ToArray();
                }
                finally
                {
                    _forecastSync.Release();
                }

                Model.WeatherForecast[] newForecast = await RequestForecast();

                _timer.Change(TimeSpan.FromSeconds(10), Timeout.InfiniteTimeSpan);

                return newForecast;
            }
        }

        private async Task AsyncTimerCallback()
        {
            var activity = Otel.ActivitySource.StartActivity("async_timer_refresh_forecast");
            try
            {
                _logger.LogInformation("Timer Callback called");

                Model.WeatherForecast[] newForecast = await RequestForecast();

                await _forecastSync.WaitAsync();
                try
                {
                    _forecast.Clear();
                    _forecast.AddRange(newForecast);
                    _logger.LogInformation("Forecast updated in background");
                }
                finally
                {
                    _forecastSync.Release();
                }
            }
            finally
            {
                activity?.Dispose();
                try
                {
                    _timer.Change(TimeSpan.FromSeconds(10), Timeout.InfiniteTimeSpan);
                }
                catch (ObjectDisposedException)
                {
                }
            }
        }

        private void TimerCallback(object? state)
        {
            _ = AsyncTimerCallback();
        }
    }
}
