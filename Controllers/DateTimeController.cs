﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_XML_XSLT.Models;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace API_XML_XSLT.Controllers
{
    [Route("tootaja")]
    [ApiController]
    public class DateTimeController : ControllerBase
    {
        private readonly TootajaDbContext _context;

        public DateTimeController(TootajaDbContext context)
        {
            _context = context;
        }

        // GET:/tootaja/andmed
        // Saame näha kõik andmed sellest tootajast
        [HttpGet("andmed")]
        public async Task<ActionResult<Tootaja>> GetTootaja()
        {
            var userId = GetUserIdFromHeader(HttpContext);

            // Kontrollime, et on olemas selle töötaja
            var tootaja = await _context.Tootajad.FindAsync(userId);
            if (tootaja == null)
            {
                return Unauthorized("Töötajast ei leitud.");
            }


            return Ok(tootaja);
        }

        // GET:/tootaja/tooaeg
        // Saame näha kõik tööaeg
        [HttpGet("tooaeg")]
        public async Task<IActionResult> GetWorkHours()
        {
            var userId = GetUserIdFromHeader(HttpContext);

            // Kontrollime, et on olemas selle töötaja
            var user = await _context.Tootajad.FindAsync(userId);
            if (user == null)
            {
                return Unauthorized("Kasutajat ei leitud.");
            }

            // Saame saada tööaega selle töötajast
            var workHours = await _context.IgapaevaAndmed
            .Where(x => x.TootajaId == userId)
            .Select(x => new
            {
                x.Id,
                x.Kuupaev,
                x.Too_algus,
                x.Too_lypp
            })
            .ToListAsync();

            if (workHours == null || workHours.Count == 0)
            {
                return NotFound("Kasutaja tööaega ei leitud.");
            }

            return Ok(workHours);
        }

        // POST: /tootaja/tooaeg_lisamine
        // Lisamine uus tööaeg
        [HttpPost("tooaeg_lisamine")]
        public async Task<IActionResult> AddWorkHours([FromBody] TooAeg_andmed tooAeg_Andmed)
        {
            var userId = GetUserIdFromHeader(HttpContext);
            int tootajaId = Convert.ToInt32(userId);

            // Kontrollime, et on olemas selle töötaja
            var user = await _context.Tootajad.FindAsync(userId);
            if (user == null)
            {
                return Unauthorized("Kasutajat ei leitud.");
            }

            // Преобразуем строки времени в TimeSpan с обработкой ошибок
            TimeSpan tooAlgus = TimeSpan.Zero;
            TimeSpan tooLypp = TimeSpan.Zero;

            if (!string.IsNullOrEmpty(tooAeg_Andmed.TooAlgus))
            {
                try
                {
                    tooAlgus = TimeSpan.Parse(tooAeg_Andmed.TooAlgus); // Преобразование строки в TimeSpan
                }
                catch (FormatException)
                {
                    return BadRequest("Неверный формат времени для TooAlgus.");
                }
            }

            if (!string.IsNullOrEmpty(tooAeg_Andmed.TooLypp))
            {
                try
                {
                    tooLypp = TimeSpan.Parse(tooAeg_Andmed.TooLypp); // Преобразование строки в TimeSpan
                }
                catch (FormatException)
                {
                    return BadRequest("Неверный формат времени для TooLypp.");
                }
            }

            // Создаем запись в базе данных
            var workHours = new Igapaeva_andmed
            {
                TootajaId = tootajaId,
                Kuupaev = tooAeg_Andmed.Kuupaev,
                Too_algus = tooAlgus,
                Too_lypp = tooLypp
            };


            _context.IgapaevaAndmed.Add(workHours);

            await _context.SaveChangesAsync();

            return Ok("Tööaeg on lisatud");
        }

        // PUT: /tootaja/tooaeg_muudamine
        // Muudamine tööaega andmed töötajast id-ga
        [HttpPut("tooaeg_muudamine")]
        public async Task<IActionResult> UpdateWorkHour([FromBody] TooAeg_andmed tooAeg_Andmed, [FromQuery] int tooaegaId)
        {
            var userId = GetUserIdFromHeader(HttpContext);

            if (userId <= 0)
            {
                return Unauthorized("UserId puudub või on vale.");
            }

            var user = await _context.Tootajad.FindAsync(userId);
            if (user == null)
            {
                return Unauthorized("Kasutajat ei leitud.");
            }

            var existingWorkHour = await _context.IgapaevaAndmed.FindAsync(tooaegaId);
            if (existingWorkHour == null)
            {
                return NotFound("Tööaeg ei ole leitud.");
            }

            // Проверка соответствия рабочего времени пользователю
            if (existingWorkHour.TootajaId != userId)
            {
                return Unauthorized("Te ei saa muuta teiste töötajate tööaega.");
            }

            TimeSpan tooAlgus = TimeSpan.Zero;
            TimeSpan tooLypp = TimeSpan.Zero;

            try
            {
                if (!string.IsNullOrEmpty(tooAeg_Andmed.TooAlgus))
                {
                    tooAlgus = TimeSpan.Parse(tooAeg_Andmed.TooAlgus);
                }

                if (!string.IsNullOrEmpty(tooAeg_Andmed.TooLypp))
                {
                    tooLypp = TimeSpan.Parse(tooAeg_Andmed.TooLypp);
                }
            }
            catch (FormatException)
            {
                return BadRequest("Неверный формат времени.");
            }

            existingWorkHour.Kuupaev = tooAeg_Andmed.Kuupaev;
            existingWorkHour.Too_algus = tooAlgus;
            existingWorkHour.Too_lypp = tooLypp;

            try
            {
                await _context.SaveChangesAsync();
                return Ok("Трудовой час успешно обновлён.");
            }
            catch (DbUpdateException dbEx)
            {
                // Логируем ошибку
                return StatusCode(500, "Ошибка при сохранении данных в базе.");
            }
            catch (Exception ex)
            {
                // Логируем другие ошибки
                return StatusCode(500, "Неизвестная ошибка.");
            }
        }


        // DELETE: /tooaeg_kustutamine
        // Kustutamine konkreetselt tööaeg töötajast id-ga
        [HttpDelete("tooaeg_kustutamine")]
        public async Task<IActionResult> DeleteWorkHour([FromQuery] int tooaegaId)
        {
            var userId = GetUserIdFromHeader(HttpContext);

            if (userId <= 0)
            {
                return Unauthorized("UserId puudub või on vale.");
            }

            var user = await _context.Tootajad.FindAsync(userId);
            if (user == null)
            {
                return Unauthorized("Kasutajat ei leitud.");
            }

            var workHour = await _context.IgapaevaAndmed.FindAsync(tooaegaId);
            if (workHour == null)
            {
                return NotFound("Töötundi ei leitud.");
            }

            // Kontrollida, et töötund kuulub sellele töötajale
            if (workHour.TootajaId != userId)
            {
                return Unauthorized("Te ei saa kustutada teiste töötajate tööaega.");
            }

            _context.IgapaevaAndmed.Remove(workHour);
            await _context.SaveChangesAsync();

            return Ok("Töötund edukalt kustutatud.");
        }

        // Saada UserId päisest
        private int? GetUserIdFromHeader(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("UserId", out var userIdString) && int.TryParse(userIdString, out var userId))
            {
                return userId;
            }
            return null;
        }
    }
}
