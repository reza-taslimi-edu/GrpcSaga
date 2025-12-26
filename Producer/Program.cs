using GrpcContracts;
using Microsoft.EntityFrameworkCore;
using Producer;
using Producer.Infrustructures;
using Producer.Infrustructures.Repositories.Cities;
using Producer.ProtosServices;
using Shared;
using Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<Settings>(builder.Configuration);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ProducerDbContext>(options =>
    {
        options.UseNpgsql(connectionString);
        options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    }
);

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerGenDoc("Producer API", "v1", "My API Description");
});

builder.Services.AddScoped<ICityRepository, CityRepository>();

builder.Services.AddScoped<GrpcCityServiceClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Producer API V1");
    c.RoutePrefix = "swagger";
});

await app.EnsureDatabaseAndMigrationsAsync<ProducerDbContext>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.GetProtos();

app.ViewProto();

app.Rawproto();

app.DownloadProto();

app.Run();
