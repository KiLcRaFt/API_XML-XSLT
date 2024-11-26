using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API_XML_XSLT.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly TootajaDbContext _context;

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

        if (tootaja == null || tootaja.Salasyna != Password)
        {
            return Unauthorized();
        }

        var token = GenerateJwtToken(tootaja.Id.ToString(), tootaja.Is_admin);

        return Ok(new { token });
    }

    private string GenerateJwtToken(string userId, bool isAdmin)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

        var role = isAdmin ? "Admin" : "Worker";

        var expiryMinutes = int.Parse(_configuration["Jwt:ExpiryInMinutes"]);
        var expirationTime = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes).ToUnixTimeSeconds();

        var audiences = _configuration["Jwt:Audience"];


        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim("isAdmin", isAdmin.ToString()),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Aud, audiences),
            new Claim(JwtRegisteredClaimNames.Exp, expirationTime.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);  // Токен с временем истечения
    }

    [HttpPost("validate-token")]
    public IActionResult ValidateToken([FromHeader(Name = "Authorization")] string authorizationHeader)
    {
        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
        {
            return Unauthorized("Authorization header missing or incorrect.");
        }

        // Извлекаем токен из заголовка
        var token = authorizationHeader.Substring("Bearer ".Length).Trim();

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

        try
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };

            // Проверяем токен
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);

            // Если код до этого места выполняется, токен валиден
            return Ok("Token is valid!");
        }
        catch (SecurityTokenExpiredException)
        {
            // Если токен просрочен
            return Unauthorized("Token has expired.");
        }
        catch (SecurityTokenInvalidAudienceException)
        {
            // Если аудитория токена не совпадает
            return Unauthorized("Invalid audience.");
        }
        catch (SecurityTokenInvalidIssuerException)
        {
            // Если издатель токена не совпадает
            return Unauthorized("Invalid issuer.");
        }
        catch (SecurityTokenException)
        {
            // Все другие ошибки токена
            return Unauthorized("Invalid token.");
        }
        catch (Exception ex)
        {
            // Другие ошибки
            return Unauthorized("Error: " + ex.Message);
        }
    }

    [HttpPost("validate-token2")]
    public IActionResult ValidateToken2(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            // Параметры для проверки токена с жестко заданными значениями
            var key = Encoding.UTF8.GetBytes("SuperMegaSecretKeyJou521234567890");
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "local-api",
                ValidAudience = "local-users",
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };

            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
            // Если код до этого места выполняется, токен валиден
            return Ok("Token is valid!");
        }
        catch (SecurityTokenException ex)
        {
            // Токен неверный или подпись не совпадает
            return Unauthorized($"Invalid token: {ex.Message}");
        }
        catch (Exception ex)
        {
            // Другие ошибки
            return Unauthorized($"Error: {ex.Message}");
        }
    }
}
