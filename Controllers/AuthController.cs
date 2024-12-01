using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Text;
using API_XML_XSLT.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Microsoft.AspNetCore.Mvc.Filters;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly TootajaDbContext _context;

    public AuthController(TootajaDbContext context)
    {
        _context = context;
    }

    [HttpPost("login")]
    public IActionResult Login(string email, string password)
    {
        var user = _context.Tootajad.FirstOrDefault(u => u.Email == email && u.Salasyna == password);
        if (user == null)
        {
            return Unauthorized("Invalid username or password.");
        }

        HttpContext.Session.SetString("UserId", user.Id.ToString());
        HttpContext.Session.SetString("IsAdmin", user.Is_admin.ToString());

        Console.WriteLine($"Login attempt: Email={email}, Password={password}");

        return Ok(new { Message = "Login successful", IsAdmin = user.Is_admin, Id = user.Id });
    }

    [HttpGet("profile")]
    public IActionResult GetProfile()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return Unauthorized("User is not logged in.");
        }

        var user = _context.Tootajad.Find(userId);
        if (user == null)
        {
            return Unauthorized("User not found.");
        }

        return Ok(new
        {
            user.Id,
            user.Email,
            Role = HttpContext.Session.GetString("Role")
        });
    }

    [HttpGet("validate-session")]
    public IActionResult ValidateSession()
    {
        var userId = HttpContext.Session.GetString("UserId");
        var isAdmin = HttpContext.Session.GetString("IsAdmin");

        if (userId == null)
        {
            return Unauthorized("Session not found.");
        }

        return Ok(new { UserId = userId, IsAdmin = bool.Parse(isAdmin) });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return Ok("Logged out successfully.");
    }
}
public class AdminOnlyAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var isAdmin = context.HttpContext.Session.GetString("IsAdmin");
        if (isAdmin != "True")
        {
            context.Result = new ForbidResult();
        }
    }
}
