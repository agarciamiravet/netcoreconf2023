using BuildingBlocks.Observability;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetCoreConfMad2023_API;
using NetCoreConfMad2023_API.Data;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddHealthChecks();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Configure database settings
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<OnlineShoppingContext>(options => options.UseSqlServer(
    connectionString,
    sqlServerOptions => sqlServerOptions.CommandTimeout(300)    
    ));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//Config Redis connection
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetValue<string>("CacheSettings:ConnectionString");
});

//Observability
builder.AddObservabilityApi();

var app = builder.Build();

app.MapHealthChecks("/healthz");

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseAuthorization();

app.MapControllers();

app.CreateDbIfNotExists();

app.Run();


