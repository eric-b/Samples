using Demo.WeatherForecastApi.EntityFramework.Entity;

namespace Demo.WeatherForecastApi.EntityFramework
{
    public static class DbInitializer
    {
        public static void Initialize(SummaryDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Summaries.Any())
            {
                return;
            }

            var summaries = new Summary[]
            {
                new Summary { Label = "Freezing" },
                new Summary { Label = "Bracing" },
                new Summary { Label = "Chilly" },
                new Summary { Label = "Cool" },
                new Summary { Label = "Mild" },
                new Summary { Label = "Warm" },
                new Summary { Label = "Balmy" },
                new Summary { Label = "Hot" },
                new Summary { Label = "Sweltering" },
                new Summary { Label = "Scorching" }
            };

            foreach (Summary s in summaries)
            {
                context.Summaries.Add(s);
            }
            context.SaveChanges();
        }
    }
}
