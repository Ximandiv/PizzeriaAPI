using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Pizzeria.Database;
using Pizzeria.Database.Models;

namespace TestProject1.Integration;

[Trait("Category", "Integration")]
public class UserTest(PizzeriaWebAppFactory<Program> factory) 
    : IClassFixture<PizzeriaWebAppFactory<Program>>
{
    private readonly HttpClient _client = factory.GetAppClient();

    [Fact]
    public async Task Should_Get_All_With_Correct_Amount()
    {
        var response = await _client.GetAsync("/api/user/list");
        
        response.EnsureSuccessStatusCode();
        
        var responseBody = await response.Content.ReadAsStringAsync();
        var listOfUsers = JsonConvert.DeserializeObject<List<User>>(responseBody);
        
        using var scope = factory.Server.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PizzeriaContext>();
        
        int userCount = await context.Users.CountAsync();

        listOfUsers.Should().NotBeNull();
        listOfUsers!.Count.Should().Be(userCount);
    }

    [Fact]
    public async Task Should_Get_ById_Correct_Entity()
    {
        using var scope = factory.Server.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PizzeriaContext>();

        User? user = new() 
        { 
            Name = "UniqueUserTest", 
            Email = "test@email.com", 
            Phone = "123 4567 891",
            Address = "Street Test # Test - Test"
        };
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
        user = await context.Users.FirstOrDefaultAsync(u => u.Name == user.Name);

        var response = await _client.GetAsync($"/api/user/{user!.Id}");

        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        var userResponse = JsonConvert.DeserializeObject<User>(responseBody);

        userResponse.Should().BeEquivalentTo(user);
    }
}