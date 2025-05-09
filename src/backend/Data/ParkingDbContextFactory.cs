using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TriviumParkingApp.Backend.Data
{
    /// <summary>
    /// Factory for creating ParkingDbContext instances during design time (e.g., for EF Core Migrations).
    /// Reads configuration from local.settings.json relative to the project root.
    /// </summary>
    public class ParkingDbContextFactory : IDesignTimeDbContextFactory<ParkingDbContext>
    {
        public ParkingDbContext CreateDbContext(string[] args)
        {
            // Build configuration
            // Set base path to the application's base directory (usually the output folder like bin/Debug/net8.0)
            // where local.settings.json should be copied.
            string basePath = AppContext.BaseDirectory;

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("local.settings.json", optional: false, reloadOnChange: true)
                .Build();

            // Get connection string from the "Values" section in local.settings.json
            var connectionString = configuration.GetValue<string>("Values:SqlAzureConnectionString");
            Console.WriteLine(connectionString);
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Could not find 'SqlAzureConnectionString' in local.settings.json");
            }


            // Create DbContext options
            var optionsBuilder = new DbContextOptionsBuilder<ParkingDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            // Return new instance of DbContext
            return new ParkingDbContext(optionsBuilder.Options);
        }
    }
}