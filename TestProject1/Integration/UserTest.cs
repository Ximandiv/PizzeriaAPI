using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Pizzeria.Database;
using Pizzeria.Database.Models;

namespace TestProject1.Integration;

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
        listOfUsers.Count().Should().Be(userCount);
    }
}