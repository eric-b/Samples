namespace Demo.WeatherForecastApi.Client
{
    public class WeatherForecastClient(HttpClient httpClient)
    {
        public async Task<string> GetWeather(CancellationToken cancellationToken)
        {
            using var response = await httpClient.GetAsync("api/v1/weatherforecast", cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}
