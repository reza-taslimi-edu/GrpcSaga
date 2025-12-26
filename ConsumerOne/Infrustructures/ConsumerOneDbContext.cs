using Microsoft.EntityFrameworkCore;
using Shared.Entities;

namespace ConsumerOne.Infrustructures
{
    public class ConsumerOneDbContext(DbContextOptions<ConsumerOneDbContext> options) : DbContext(options)
    {

        public DbSet<CityEntity> Cities { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Host=localhost;Port=5433;Database=ConsumerOneDb;Username=postgres;Password=postgres");
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(GetType().Assembly);

            base.OnModelCreating(builder);
        }
    }
}
