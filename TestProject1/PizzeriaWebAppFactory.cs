using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Pizzeria.Database;
using Pizzeria.Database.Seeders;

namespace TestProject1;

public class PizzeriaWebAppFactory<TProgram> 
    : WebApplicationFactory<TProgram> where TProgram : class
{
    private static bool _dbInitialized;
    private HttpClient _client;
    protected IConfiguration? Configuration { private set; get; }

    public PizzeriaWebAppFactory()
    {
        _client = CreateClient();
    }
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<PizzeriaContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            Configuration = new ConfigurationBuilder()
                .AddUserSecrets<PizzeriaWebAppFactory<Program>>()
                .AddEnvironmentVariables()
                .Build();

            if(!_dbInitialized)
            {
                lock(this)
                {
                    if(!_dbInitialized)
                    {
                        var connectionString = Configuration.GetConnectionString("DefaultConnection")
                            ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                            ?? throw new InvalidOperationException("Connection string not configured");

                        services.AddDbContext<PizzeriaContext>(options =>
                        {
                            options.UseMySql
                            (
                                connectionString, 
                                ServerVersion.AutoDetect(connectionString)
                            );
                        });

                        var serviceProvider = services.BuildServiceProvider();
                        try
                        {
                            using (var scope = serviceProvider.CreateScope())
                            {
                                var dbContext = scope.ServiceProvider.GetRequiredService<PizzeriaContext>();
                                int maxRetries = 5;
                                for (int attempt = 1; attempt <= maxRetries; attempt++)
                                {
                                    try
                                    {
                                        dbContext.Database.Migrate();
                                        break;
                                    }
                                    catch (Exception)
                                    {
                                        if (attempt == maxRetries)
                                            throw;
                                        Console.WriteLine($"Attempt {attempt} failed, retrying...");
                                        Thread.Sleep(5000);
                                    }
                                }

                                TestContextSeeder testSeeder = new TestContextSeeder(dbContext);
                                testSeeder.Seed();
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new InvalidOperationException("Database migration failed during test setup", ex);
                        }
                        _dbInitialized = true;
                    }
                }
            }
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _client?.Dispose();
        
        base.Dispose(disposing);
    }

    public HttpClient GetAppClient() => _client;
}