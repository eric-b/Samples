using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Otel = AspNetCoreWebApp.Tracing.OpenTelemetry;

namespace AspNetCoreWebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly Components.WeatherForecastService _weatherForecast;

        public WeatherForecastController(Components.WeatherForecastService weatherForecast, ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
            _weatherForecast = weatherForecast;
        }

        [HttpGet]
        public async Task<IEnumerable<Model.WeatherForecast>> Get()
        {
            using (Otel.ActivitySource.StartActivity("get_weather_forecast_from_controller"))
            {
                _logger.LogInformation("WeatherForecast Controller called");
                return await _weatherForecast.GetWeatherForecast();
            }
        }
    }
}
