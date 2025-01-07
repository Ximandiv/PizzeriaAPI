using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Pizzeria.Database;
using Pizzeria.Database.Seeders;
using Pizzeria.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>();

builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection("MongoDB"));

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

var connectionString = builder.Configuration["my_secret"] ?? builder.Configuration["AppSettings:TestConnection"];

builder.Services.AddDbContext<PizzeriaContext>(options =>
{
    options.UseMySql
    (
        connectionString,
        Microsoft.EntityFrameworkCore.ServerVersion.AutoDetect(connectionString)
    );
});

builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
{
    var settings = serviceProvider.GetService<IOptions<MongoSettings>>()!.Value;

    var pack = new ConventionPack { new CamelCaseElementNameConvention() };
    ConventionRegistry.Register("elementNameConvention", pack, x => true);

    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddScoped<IMongoDatabase>(serviceProvider =>
{
    var client = serviceProvider.GetService<IMongoClient>();
    var settings = serviceProvider.GetService<IOptions<MongoSettings>>()!.Value;
    return client!.GetDatabase(settings.Database);
});

builder.Services.AddScoped<OrdersContext>();

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