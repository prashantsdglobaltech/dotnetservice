namespace Campaigns.API.Infrastructure
{
    using System;
    using System.IO;
    using EntityConfigurations;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Campaigns.API.Model;
    using Microsoft.Extensions.Configuration;

    public class MarketingContext : DbContext
    {
        public MarketingContext(DbContextOptions<MarketingContext> options) : base(options)
        {    
        }

        public DbSet<Campaign> Campaigns { get; set; }

        public DbSet<Rule> Rules { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new CampaignEntityTypeConfiguration());
           
        }
    }

    public class MarketingContextDesignFactory : IDesignTimeDbContextFactory<MarketingContext>
    {
        public MarketingContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration["ConnectionString"];
            var optionsBuilder = new DbContextOptionsBuilder<MarketingContext>()
                .UseSqlServer(connectionString);
            return new MarketingContext(optionsBuilder.Options);
        }
    }
}