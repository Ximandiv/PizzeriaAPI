using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Pizzeria.Database;

namespace TestProject1;

public class PizzeriaWebAppFactory<TProgram> 
    : WebApplicationFactory<TProgram> where TProgram : class
{
    private readonly HttpClient _client;

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
            
            services.AddDbContext<PizzeriaContext>(options =>
            {
                options.UseMySql
                (
                    "server=localhost;database=PizzeriaTest;user=root;password=3GdhXn+0ai[1uSk1<HfJ", 
                    ServerVersion.AutoDetect("server=localhost;database=PizzeriaTest;user=root;password=3GdhXn+0ai[1uSk1<HfJ")
                );
            });

        });
    }

    public HttpClient GetAppClient() => _client;
}