using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Text;
using API_XML_XSLT.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Microsoft.AspNetCore.Mvc.Filters;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly TootajaDbContext _context;

    public AuthController(TootajaDbContext context)
    {
        _context = context;
    }

    // POST: /auth/login
    // Logime sisse
    [HttpPost("login")]
    // Logime sisse mailiga ja salasõnaga
    public IActionResult Login([FromBody] LoginRequest loginRequest)
    {
        var user = _context.Tootajad.FirstOrDefault(u => u.Email == loginRequest.Email && u.Salasyna == loginRequest.Password);
        if (user == null)
        {
            return Unauthorized("Vale kasutajanimi või salasõna.");
        }

        // Lisame sessioni andmed
        HttpContext.Session.SetString("UserId", user.Id.ToString());
        HttpContext.Session.SetString("IsAdmin", user.Is_admin.ToString());

        return Ok(new { Message = "Sisselogimine õnnestus", IsAdmin = user.Is_admin, Id = user.Id });
    }

    // GET: /auth/profile
    // Saatmine sessioni andmed
    [HttpGet("profile")]
    public IActionResult GetProfile()
    {
        var userId = GetUserIdFromHeader(HttpContext);

        if (userId == null)
        {
            return Unauthorized("Kasutaja ei ole logitud.");
        }

        var user = _context.Tootajad.FirstOrDefault(u => u.Id == userId);
        if (user == null)
        {
            return NotFound("Kasutajat ei leitud.");
        }

        return Ok(new
        {
            user.Id,
            user.Email,
            IsAdmin = user.Is_admin
        });
    }

    //POST: /auth/logout
    // Logime välja
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        //Kustutamine session
        HttpContext.Session.Clear();
        return Ok("Edukalt välja logitud.");
    }

    private int? GetUserIdFromHeader(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("UserId", out var userIdString) && int.TryParse(userIdString, out var userId))
        {
            return userId;
        }
        return null;
    }
}

// Autibuut mis annab meil admini funktsionaalsus
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
