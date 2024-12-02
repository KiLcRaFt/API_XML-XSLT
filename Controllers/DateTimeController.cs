using Microsoft.AspNetCore.Mvc;
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

        // GET:/tootaja/work-hours
        // Saame näha kõik tööaeg
        [HttpGet("work-hours")]
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

        // POST: /tootaja/work-hour_lisamine
        // Lisamine uus tööaeg
        [HttpPost("work-hour_lisamine")]
        public async Task<IActionResult> AddWorkHours(DateOnly kuupaev, TimeOnly tooAlgus, TimeOnly tooLypp)
        {
            var userId = GetUserIdFromHeader(HttpContext);
            int tootajaId = Convert.ToInt32(userId);

            // Kontrollime, et on olemas selle töötaja
            var user = await _context.Tootajad.FindAsync(userId);
            if (user == null)
            {
                return Unauthorized("Kasutajat ei leitud.");
            }

            var workHours = new Igapaeva_andmed
            {
                TootajaId = tootajaId,
                Kuupaev = kuupaev,
                Too_algus = tooAlgus,
                Too_lypp = tooLypp
            };

            _context.IgapaevaAndmed.Add(workHours);

            await _context.SaveChangesAsync();

            return Ok("Tööaeg on lisatud");
        }

        // PUT: /tootaja/update-work-hour/{id}
        // Muudamine tööaega andmed töötajast id-ga
        [HttpPut("update-work-hour/{id}")]
        public async Task<IActionResult> UpdateWorkHour(int tooaegaId, DateOnly kuupaev, TimeOnly tooAlgus, TimeOnly tooLypp)
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

            // Kontrollida, et töötund kuulub sellele töötajale
            if (existingWorkHour.TootajaId != userId)
            {
                return Unauthorized("Te ei saa muuta teiste töötajate tööaega.");
            }

            existingWorkHour.Kuupaev = kuupaev;
            existingWorkHour.Too_algus = tooAlgus;
            existingWorkHour.Too_lypp = tooLypp;

            await _context.SaveChangesAsync();

            return Ok("Töötund edukalt uuendatud.");
        }

        // DELETE: /admin/delete-work-hour/{id}
        // Kustutamine konkreetselt tööaeg töötajast id-ga
        [HttpDelete("delete-work-hour/{id}")]
        public async Task<IActionResult> DeleteWorkHour(int tooaegaId)
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
