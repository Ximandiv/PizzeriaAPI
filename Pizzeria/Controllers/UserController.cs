using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pizzeria.Database.Models;
using Pizzeria.DTOs.Users;
using Pizzeria.Services;

namespace Pizzeria.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController
    (UserService userService, TokenService tokenService) 
    : ControllerBase
{
    [HttpGet("list")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetAll()
    {
        IEnumerable<User?>? userList = await userService.GetAll();

        if(userList == null 
            || userList.Count() == 0)
            return NotFound();
        
        return Ok(userList.Select(u => u!.ToResponse()).ToList());
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetById(int id)
    {
        User? user = await userService.Get(id);

        if(user == null) return NotFound();

        return Ok(user.ToResponse());
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> LogIn([FromBody] LoginDTO loginModel)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        User? user = await userService.GetByEmail(loginModel.Email);

        if (user is null) return BadRequest("Invalid credentials");

        var validPassword = BCrypt.Net.BCrypt.Verify(loginModel.Password, user.Password);

        if (!validPassword) return BadRequest("Invalid credentials");

        var jwt = tokenService.GenerateJWT(user);

        return Ok(jwt);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(UserDTO model)
    {
        if(!ModelState.IsValid)
            return BadRequest(ModelState);

        User entityModel = model.ToEntity();

        await userService.Create(entityModel);

        var response = entityModel.ToResponse();

        return CreatedAtAction(nameof(GetById),  new { id = entityModel.Id }, response);
    }
}