using Microsoft.EntityFrameworkCore;
using Shared.Entities;

namespace Producer.Infrustructures
{
    public class ProducerDbContext(DbContextOptions<ProducerDbContext> options) : DbContext(options)
    {

        public DbSet<CityEntity> Cities { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Host=localhost;Port=5433;Database=ProducerDb;Username=postgres;Password=postgres");
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(GetType().Assembly);

            base.OnModelCreating(builder);
        }
    }
}
