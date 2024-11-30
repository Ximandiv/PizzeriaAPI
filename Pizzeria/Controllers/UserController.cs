using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pizzeria.Database;
using Pizzeria.Database.Models;
using Pizzeria.Services;

namespace Pizzeria.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController
    (UserService userService) 
    : ControllerBase
{
    [HttpGet("list")]
    public async Task<IActionResult> GetAll()
    {
        IEnumerable<User?> userList = await userService.GetAll();

        if(userList == null
            || userList.Count() == 0)
            return NotFound();
        
        return Ok(userList);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        User? user = await userService.Get(id);

        if(user == null) return NotFound();

        return Ok(user);
    }
}