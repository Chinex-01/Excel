using ClosedXML.Excel;
using Excel;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using Swashbuckle.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();


string connectionString =
    builder.Configuration.GetConnectionString("EmployeeDb");

// Program.cs — replace the AddScoped factory with this:

builder.Services.AddScoped<SqlConn>();
builder.Services.AddScoped<SqlConn2>();

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