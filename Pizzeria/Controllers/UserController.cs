using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pizzeria.Database.Models;
using Pizzeria.DTOs;
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
        IEnumerable<User?>? userList;
        try
        {
            userList = await userService.GetAll();
        }
        catch (Exception ex)
        {
            return StatusCode(500, UserError.GetAll);
        }

        if(userList == null 
            || userList.Count() == 0)
            return NotFound();

        var response = userList.Select(u => u!.ToResponse()).ToList();

        return Ok(response);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetById(int id)
    {
        User? user;
        try
        {
            user = await userService.Get(id);
        }
        catch(Exception ex)
        {
            return StatusCode(500, UserError.GetByEmail);
        }

        if(user is null)
            return NotFound();

        var response = user.ToResponse();

        return Ok(response);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> LogIn([FromBody] LoginDTO loginModel)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            User? user = await userService.GetByEmail(loginModel.Email);

            if (user is null) return BadRequest(UserError.InvalidCredentials);

            var validPassword = BCrypt.Net.BCrypt.Verify(loginModel.Password, user.Password);

            if (!validPassword) return BadRequest(UserError.InvalidCredentials);

            var jwt = tokenService.GenerateJWT(user);

            return Ok(jwt);
        }
        catch (Exception ex)
        {
            return StatusCode(500, UserError.LogIn);
        }
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(UserDTO model)
    {
        if(!ModelState.IsValid)
            return BadRequest(ModelState);

        User entityModel = model.ToEntity();

        try
        {
            await userService.Create(entityModel);

            await userService.AddDefaultRole(entityModel);
        }
        catch(Exception ex)
        {
            return StatusCode(500, UserError.Create);
        }

        var response = entityModel.ToResponse();

        return CreatedAtAction(nameof(GetById),  new { id = entityModel.Id }, response);
    }
}