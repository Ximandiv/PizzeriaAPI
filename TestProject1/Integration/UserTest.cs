using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Pizzeria.Database;
using Pizzeria.Database.Models;
using Pizzeria.DTOs.Users;
using System.Text;
using TestProject1.Responses;

namespace TestProject1.Integration;

[Trait("Category", "Integration")]
public class UserTest(PizzeriaWebAppFactory<Program> factory) 
    : IClassFixture<PizzeriaWebAppFactory<Program>>
{
    private readonly HttpClient _client = factory.GetAppClient();
    private readonly LoginDTO _loginDTO = new()
    {
        Email = "admin@test.com",
        Password = "123456789!@Abc"
    };

    [Fact]
    public async Task Should_Get_All_With_Correct_Amount()
    {
        using var scope = factory.Server.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PizzeriaContext>();

        var loginRequest = new HttpRequestMessage(HttpMethod.Post, "/api/user/login")
        {
            Content = new StringContent(JsonConvert.SerializeObject(_loginDTO), Encoding.UTF8, "application/json"),
        };
        var loginResponse = await _client.SendAsync(loginRequest);
        var loginResponseBody = await loginResponse.Content.ReadAsStringAsync();
        var responseObject = JsonConvert.DeserializeObject<TestResponseObject<string>>(loginResponseBody);
        var jwt = responseObject!.Message;

        var getRequest = new HttpRequestMessage(HttpMethod.Get, "/api/user/list");
        getRequest.Headers.Add("Authorization", $"Bearer {jwt}");

        var response = await _client.SendAsync(getRequest);
        
        response.EnsureSuccessStatusCode();
        
        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<TestResponseObject<List<User>>>(responseBody);
        
        int userCount = await context.Users.CountAsync();

        result.Should().NotBeNull();
        result!.Message.Should().NotBeNull();
        result.Message!.Count.Should().Be(userCount);
    }

    [Fact]
    public async Task Should_Get_ById_Correct_Entity()
    {
        using var scope = factory.Server.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PizzeriaContext>();

        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == 1);

        var loginRequest = new HttpRequestMessage(HttpMethod.Post, "/api/user/login")
        {
            Content = new StringContent(JsonConvert.SerializeObject(_loginDTO), Encoding.UTF8, "application/json"),
        };
        var loginResponse = await _client.SendAsync(loginRequest);
        var loginResponseBody = await loginResponse.Content.ReadAsStringAsync();
        var responseObject = JsonConvert.DeserializeObject<TestResponseObject<string>>(loginResponseBody);
        var jwt = responseObject!.Message;

        var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/user/{user!.Id}");
        getRequest.Headers.Add("Authorization", $"Bearer {jwt}");

        var response = await _client.SendAsync(getRequest);

        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        var userResponse = JsonConvert.DeserializeObject<TestResponseObject<User>>(responseBody);

        userResponse.Should().NotBeNull();
        userResponse!.Message.Should().NotBeNull();
        userResponse.Message.Should().BeEquivalentTo(user.ToResponse());
    }
}