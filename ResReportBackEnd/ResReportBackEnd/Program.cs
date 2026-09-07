using ResReportBackEnd.services.reports;
using ResReportBackEnd.services.reports.ClientReportPack;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddTransient<IReportService, ClientReportPack>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders("Content-Disposition");
    });
});

var app = builder.Build();

app.UseCors("AllowFrontend");

app.MapControllers();

// var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
// app.Urls.Add($"http://0.0.0.0:{port}");
app.Run();