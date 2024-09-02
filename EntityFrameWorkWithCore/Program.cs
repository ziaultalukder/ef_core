using EntityFrameWorkWithCore;
using EntityFrameWorkWithCore.Context;
using EntityFrameWorkWithCore.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddExceptionHandler<AppNoImplementedExceptionHandler>();
builder.Services.AddExceptionHandler<ExceptionHandler>();


var logger = new LoggerConfiguration()
 .ReadFrom.Configuration(builder.Configuration)
 .MinimumLevel.Debug()
 .Enrich.FromLogContext()
 .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddJwtAuthentication();
builder.Services.AddDbContext<ApplicationDbContext>(option => option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddStackExchangeRedisCache(
    option =>
    {

        var connection = builder.Configuration.GetConnectionString("Redis");
        option.Configuration = connection;
    });

builder.Services.AddTransient<IDbConnection>(db => new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAntiforgery();

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
var dd = builder.Configuration.AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsStaging() || app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//builder.Services.AddStackExchangeRedisCache(options => { options.Configuration = builder.Configuration["Redis"]; });

app.UseExceptionHandler(_ => { });
app.UseAuthorization();

app.MapControllers();

app.Run();
