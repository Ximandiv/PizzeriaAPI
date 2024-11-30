using Microsoft.EntityFrameworkCore;
using Pizzeria;
using Pizzeria.Database;
using Pizzeria.Database.Seeders;
using Pizzeria.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>();

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

var connectionString = builder.Configuration["my_secret"] ?? builder.Configuration["AppSettings:TestConnection"];

builder.Services.AddDbContext<PizzeriaContext>(options =>
{
    options.UseMySql
    (
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    );
});

builder.Services.AddScoped<UserService>();

builder.Services.AddControllers();
// Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PizzeriaContext>();
    dbContext.Database.Migrate();

    if (environment == "Development")
    {
        TestContextSeeder testSeeder = new TestContextSeeder(dbContext);
        testSeeder.Seed();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }