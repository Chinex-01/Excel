using ClosedXML.Excel;
using Excel;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using Swashbuckle.AspNetCore;
using Serilog.Context;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        path: "Logs/log-.txt",
        rollingInterval: RollingInterval.Day,   // creates log-20260722.txt, log-20260723.txt, etc.
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog(); // 👈


builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

string connectionString =
    builder.Configuration.GetConnectionString("EmployeeDb");

// Program.cs — replace the AddScoped factory with this:
builder.Services.AddScoped<SqlConn>();
builder.Services.AddScoped<SqlConn2>();
builder.Services.AddScoped<Validation>();
builder.Services.AddScoped<ReferenceNumberGenerate>();

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "Excel API V1");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();