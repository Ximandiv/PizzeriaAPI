using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pizzeria.Database;
using Pizzeria.Database.Models;

namespace Pizzeria.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(ILogger<UserController> logger, PizzeriaContext dbContext) : ControllerBase
{
    private readonly ILogger<UserController> _logger = logger;
    private readonly PizzeriaContext _context = dbContext;

    [HttpGet("list")]
    public async Task<IActionResult> Get()
    {
        IEnumerable<User> userList = await _context.Users.ToListAsync();
        
        return Ok(userList);
    }
}