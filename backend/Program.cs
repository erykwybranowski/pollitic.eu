using backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text;

namespace backend
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            
            // Load the SSL setting from the environment variable
            var enableSsl = Environment.GetEnvironmentVariable("ENABLE_SSL") == "true";
            Console.WriteLine($"ENABLE_SSL: {enableSsl}");
            
            // Select the appropriate appsettings file based on the SSL setting
            var appSettingsFile = enableSsl ? "appsettings.https.json" : "appsettings.json";
            Console.WriteLine($"Loading appsettings file: {appSettingsFile}");
            
            // Reconfigure the builder to use the selected appsettings file
            builder.Configuration.AddJsonFile(appSettingsFile, optional: false, reloadOnChange: true);
            
            builder.Services.AddControllers();

            // Configure services
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseMySql(Environment.GetEnvironmentVariable("DefaultConnection"),
                    new MySqlServerVersion(new Version(8, 0, 25)),
                    opt => opt.CommandTimeout(600)));

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "backend", Version = "v1" });
            });
            
            // Configure CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder => builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });

            var app = builder.Build();
            
            // Log app environment
            Console.WriteLine($"Application Environment: {app.Environment.EnvironmentName}");

            // Configure middleware
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            
            // Use HTTPS redirection only if SSL is enabled
            if (enableSsl)
            {
                Console.WriteLine("SSL is enabled, using HTTPS redirection.");
                app.UseHttpsRedirection();
            }
            else
            {
                Console.WriteLine("SSL is disabled, using HTTP only.");
            }

            app.UseCors("AllowAll"); // Use CORS policy
            app.UseAuthorization();
            app.MapControllers();
            Console.WriteLine("Application started.");
            app.Run();
        }
    }
}