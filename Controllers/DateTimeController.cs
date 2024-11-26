using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_XML_XSLT.Models;
using System.Security.Claims;

namespace API_XML_XSLT.Controllers
{
    [Route("api/tootaja")]
    [ApiController]
    [Authorize(Roles = "Worker")]
    public class DateTimeController : ControllerBase
    {
        private readonly TootajaDbContext _context;

        public DateTimeController(TootajaDbContext context)
        {
            _context = context;
        }

        // Получение рабочего времени текущего работника
        [HttpGet("work-hours")]
        public async Task<IActionResult> GetWorkHours()
        {
            // Получаем информацию о текущем пользователе из токена
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not found.");
            }

            // Преобразуем userId в int (предполагаем, что userId — это Id работника)
            int tootajaId;
            if (!int.TryParse(userId, out tootajaId))
            {
                return Unauthorized("Invalid user ID.");
            }

            // Получаем рабочие часы для этого работника
            var workHours = await _context.IgapaevaAndmed
                .Where(w => w.TootajaId == tootajaId)
                .ToListAsync();

            // Если данных о рабочем времени нет, можно вернуть пустой список или сообщение
            if (workHours == null || workHours.Count == 0)
            {
                return NotFound("No work hours found for this user.");
            }

            return Ok(workHours);
        }

        // Добавление нового рабочего времени
        [HttpPost("work-hours")]
        public async Task<IActionResult> AddWorkHours([FromBody] Igapaeva_andmed tooAeg)
        {
            // Получаем информацию о текущем пользователе из токена
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not found.");
            }

            // Преобразуем userId в int (предполагаем, что userId — это Id работника)
            int tootajaId = int.Parse(userId);

            // Проверка, что новый рабочий час относится к текущему работнику
            if (tooAeg.TootajaId != tootajaId)
            {
                return BadRequest("You can only add work hours for yourself.");
            }

            // Добавляем новое рабочее время в базу данных
            _context.IgapaevaAndmed.Add(tooAeg);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetWorkHours), new { id = tooAeg.Id }, tooAeg);
        }
    }
}
