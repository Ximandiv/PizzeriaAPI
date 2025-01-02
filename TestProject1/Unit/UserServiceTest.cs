using FluentAssertions;
using Moq;
using Moq.EntityFrameworkCore;
using Pizzeria.Database;
using Pizzeria.Database.Models;
using Pizzeria.Services;

namespace TestProject1.Unit;

[Trait("Category", "Unit")]
public class UserServiceTest
{
    private readonly Mock<PizzeriaContext> _mockDbContext;
    private readonly UserService _service;

    public UserServiceTest()
    {
        _mockDbContext = new Mock<PizzeriaContext>();
        _service = new(_mockDbContext.Object);
    }

    [Fact]
    public async Task Should_Get_ById_Correct_Entity()
    {
        var expectedUser = new User
        {
            Id = 1,
            Name = "User1",
            Email = "User1@email.com",
            Address = "Address",
            Phone = "123 4567 891"
        };
        _mockDbContext.Setup(c => c.Users.FindAsync(expectedUser.Id)).ReturnsAsync(expectedUser);

        var result = await _service.Get(expectedUser.Id);

        Assert.NotNull(result);
        result.Should().BeEquivalentTo(expectedUser);
    }

    [Fact]
    public async Task Should_Get_All_With_Correct_Amount()
    {
        List<User> userList = new(){
            new User { Id = 1, Name = "User1", Email = "User1@email.com", Address = "Address", Phone = "123 4567 891"},
            new User { Id = 2, Name = "User2", Email = "User2@email.com", Address = "Address", Phone = "123 4567 891"}
        };

        _mockDbContext.Setup(c => c.Users).ReturnsDbSet(userList);

        var result = await _service.GetAll();

        result.Should().HaveCount(2);

        var resultList = result.ToList();
       
        for(int i = 0;  i < resultList.Count(); i++)
        {
            Assert.Equal(userList[i].Id, resultList[i].Id);
            Assert.Equal(userList[i].Name, resultList[i].Name);
            Assert.Equal(userList[i].Email, resultList[i].Email);
            Assert.Equal(userList[i].Address, resultList[i].Address);
            Assert.Equal(userList[i].Phone, resultList[i].Phone);
        }
    }
}
