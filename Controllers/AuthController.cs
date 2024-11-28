using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Security.Claims;
using System.Text;
using API_XML_XSLT.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly TootajaDbContext _context;
    private readonly string _secretKey = "SuperMegaSecretKeyJou521234567890";  // Ваш секретный ключ
    private readonly string _issuer = "local-api";  // Указание Issuer
    private readonly string _audience = "local-users";  // Указание Audience

    public AuthController(IConfiguration configuration, TootajaDbContext context)
    {
        _configuration = configuration;
        _context = context;
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login(string Email, string Password)
    {
        var tootaja = await _context.Tootajad
            .FirstOrDefaultAsync(x => x.Email == Email);

        if (tootaja == null || !VerifyPassword(Password, tootaja.Salasyna))
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var token = GenerateJwtToken(tootaja.Id.ToString(), tootaja.Is_admin);
        return Ok(new { token });
    }

    private bool VerifyPassword(string password, string hashedPassword)
    {
        var passwordHasher = new PasswordHasher<Tootaja>();
        return passwordHasher.VerifyHashedPassword(null, hashedPassword, password) == PasswordVerificationResult.Success;
    }

    public string GenerateJwtToken(string userId, bool isAdmin)
    {
        var key = Encoding.UTF8.GetBytes(_secretKey);

        // Устанавливаем роль пользователя (Admin или Worker)
        var role = isAdmin ? "Admin" : "Worker";

        // Устанавливаем список заявок (claims)
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim("isAdmin", isAdmin.ToString()),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Aud, _audience),  // audience
            new Claim(JwtRegisteredClaimNames.Iss, _issuer)    // issuer
        };

        // Создаем токен
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _issuer,
            Audience = _audience,
            Expires = DateTime.UtcNow.AddMinutes(30),  // Устанавливаем время жизни токена
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);  // Возвращаем токен в виде строки
    }


    [HttpPost("validate-token2")]
    public IActionResult ValidateToken2(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]); // Ваш секретный ключ для подписи

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _configuration["Jwt:Issuer"],  // Укажите правильный Issuer
            ValidAudience = _configuration["Jwt:Audience"],  // Укажите правильный Audience
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };

        try
        {
            // Пытаемся валидировать токен
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);

            // Если токен валиден, возвращаем ClaimsPrincipal
            return Ok(principal);
        }
        catch (SecurityTokenException ex)
        {
            // Если токен не прошел валидацию, возвращаем ошибку
            return Unauthorized($"Invalid token: {ex.Message}");
        }
        catch (Exception ex)
        {
            // Для других ошибок
            return Unauthorized($"Error: {ex.Message}");
        }
    }
}
