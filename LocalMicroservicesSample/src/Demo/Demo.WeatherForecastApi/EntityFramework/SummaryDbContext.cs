using Demo.WeatherForecastApi.EntityFramework.Entity;
using Microsoft.EntityFrameworkCore;

namespace Demo.WeatherForecastApi.EntityFramework
{
    public class SummaryDbContext : DbContext
    {
        public SummaryDbContext(DbContextOptions<SummaryDbContext> options)
        : base(options) { }

        public DbSet<Summary> Summaries => Set<Summary>();
    }
}
