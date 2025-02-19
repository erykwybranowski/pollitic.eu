using backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text;
using backend.Services;

namespace backend
{
    public partial class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            
            // Select the appropriate appsettings file based on the SSL setting
            var appSettingsFile = "appsettings.json";
            
            // Reconfigure the builder to use the selected appsettings file
            builder.Configuration.AddJsonFile(appSettingsFile, optional: false, reloadOnChange: true);
            
            builder.Services.AddControllers();

            // Configure services
            if (builder.Environment.IsEnvironment("IntegrationTest"))
            {
                builder.Services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("InMemoryTestDb"));
            }
            else
            {
                builder.Services.AddDbContext<AppDbContext>(options =>
                    options.UseMySql(Environment.GetEnvironmentVariable("DefaultConnection"),
                        new MySqlServerVersion(new Version(8, 0, 25)),
                        opt => opt.CommandTimeout(1200)));
            }

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
            
            builder.Services.AddHttpClient();
            builder.Services.AddHostedService<PollUpdateService>();

            var app = builder.Build();
            
            // Log app environment
            Console.WriteLine($"Application Environment: {app.Environment.EnvironmentName}");

            // Configure middleware
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("AllowAll"); // Use CORS policy
            app.UseAuthorization();
            app.MapControllers();
            // Conditionally run migrations only if not in IntegrationTest environment
            if (!builder.Environment.IsEnvironment("IntegrationTest"))
            {
                Console.WriteLine("Applying migrations...");
                using (var scope = app.Services.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    dbContext.Database.Migrate();
                }
            }
            else
            {
                Console.WriteLine("Skipping migrations in IntegrationTest environment.");
            }
            Console.WriteLine("App started");
            app.Run();
        }
    }
}