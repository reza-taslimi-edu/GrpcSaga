using GrpcContracts;
using SagaOrchestrator;
using SagaOrchestrator.Commonalities;
using SagaOrchestrator.ProtosServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ConsumerSettings>(builder.Configuration.GetSection("ConsumerUrls"));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

builder.Services.AddScoped<SagaOrchestration>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGrpcService<GrpcCityService>();

app.GetProtos();

app.ViewProto();

app.Rawproto();

app.DownloadProto();

app.Run();
