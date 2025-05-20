using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;


namespace Voting.Infrastructure.Data
{
    // EF Tools при design time найдёт этот класс автоматически
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // Подхватываем appsettings.json из папки проекта (общего с migrations)
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(
                configuration.GetConnectionString("VotingDatabase")
            );

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
