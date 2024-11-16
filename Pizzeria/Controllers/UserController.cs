using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pizzeria.Database;
using Pizzeria.Database.Models;

namespace Pizzeria.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController
    (PizzeriaContext dbContext) 
    : ControllerBase
{
    [HttpGet("list")]
    public async Task<IActionResult> Get()
    {
        IEnumerable<User> userList = await dbContext.Users.ToListAsync();
        
        return Ok(userList);
    }
}