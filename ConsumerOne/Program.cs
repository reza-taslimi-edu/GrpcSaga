using ConsumerOne.Infrustructures;
using ConsumerOne.Infrustructures.Repositories.Cities;
using ConsumerOne.ProtosServices;
using GrpcContracts;
using Microsoft.EntityFrameworkCore;
using Shared;
using Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ConsumerOneDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
}
);

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerGenDoc("Consumer One API", "v1", "My API Description");
});

builder.Services.AddTransient<ICityRepository, CityRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Consumer One API V1");
    c.RoutePrefix = "swagger";
});

await app.EnsureDatabaseAndMigrationsAsync<ConsumerOneDbContext>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGrpcService<GrpcCityService>();

app.GetProtos();

app.ViewProto();

app.Rawproto();

app.DownloadProto();

app.Run();
