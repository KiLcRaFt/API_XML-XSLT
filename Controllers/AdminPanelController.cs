using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_XML_XSLT.Models;
using System.Linq;
using System.Threading.Tasks;

namespace API_XML_XSLT.Controllers
{
    [ApiController]
    [AdminOnly] // Sellesed endpointid saab kastada ainult admin
    [Route("admin")]
    public class AdminPanelController : ControllerBase
    {
        private readonly TootajaDbContext _context;

        public AdminPanelController(TootajaDbContext context)
        {
            _context = context;
        }

        // GET: /admin/tooaeg
        // Saatmine kõik töötajast tööaega andmed
        [HttpGet("tooaeg")]
        public async Task<IActionResult> GetAllWorkHours()
        {
            var workHours = await _context.IgapaevaAndmed.ToListAsync();

            if (workHours == null || !workHours.Any())
            {
                return NotFound("Töötundide andmeid ei leitud.");
            }

            return Ok(workHours);
        }

        // GET: /admin/tootajad
        // Saatmine kõik töötajad
        [HttpGet("tootajad")]
        public async Task<IActionResult> GetAllWorkers()
        {
            var workers = await _context.Tootajad.ToListAsync();

            if (workers == null || !workers.Any())
            {
                return NotFound("Töötajaid ei leitud.");
            }

            return Ok(workers);
        }

        // POST: /admin/tooaeg_lisamine
        // Lisamine tööaeg töötajatele id-ga
        [HttpPost("tooaeg_lisamine")]
        public async Task<IActionResult> AddWorkHour(int userId, DateTime kuupaev, TimeSpan tooAlgus, TimeSpan tooLypp)
        {
            if (userId <= 0)
            {
                return Unauthorized("UserId puudub või on vale.");
            }

            var user = await _context.Tootajad.FindAsync(userId);
            if (user == null)
            {
                return Unauthorized("Kasutajat ei leitud.");
            }

            var newWorkHour = new Igapaeva_andmed
            {
                TootajaId = userId,
                Kuupaev = kuupaev,
                Too_algus = tooAlgus,
                Too_lypp = tooLypp
            };

            _context.IgapaevaAndmed.Add(newWorkHour);
            await _context.SaveChangesAsync();

            return Ok(newWorkHour);
        }

        // PUT: /admin/tooaeg_muudamine
        // Muudamine tööaega andmed töötajast id-ga
        [HttpPut("tooaeg_muudamine")]
        public async Task<IActionResult> UpdateWorkHour(int tooaegaId, int tootajaId, DateTime kuupaev, TimeSpan tooAlgus, TimeSpan tooLypp)
        {
            if (tootajaId <= 0)
            {
                return Unauthorized("UserId puudub või on vale.");
            }

            var user = await _context.Tootajad.FindAsync(tootajaId);
            if (user == null)
            {
                return Unauthorized("Kasutajat ei leitud.");
            }

            var existingWorkHour = await _context.IgapaevaAndmed.FindAsync(tooaegaId);
            if (existingWorkHour == null)
            {
                return NotFound("Tööaeg ei ole leitud.");
            }

            existingWorkHour.Kuupaev = kuupaev;
            existingWorkHour.Too_algus = tooAlgus;
            existingWorkHour.Too_lypp = tooLypp;

            await _context.SaveChangesAsync();

            return Ok("Töötund edukalt uuendatud.");
        }

        // DELETE: /admin/tooaeg_kustutamine
        // Kustutamine konkreetselt tööaeg töötajast id-ga
        [HttpDelete("tooaeg_kustutamine")]
        public async Task<IActionResult> DeleteWorkHour(int tooaegaId, int tootajaId)
        {
            if (tootajaId <= 0)
            {
                return Unauthorized("UserId puudub või on vale.");
            }

            var user = await _context.Tootajad.FindAsync(tootajaId);
            if (user == null)
            {
                return Unauthorized("Kasutajat ei leitud.");
            }

            var workHour = await _context.IgapaevaAndmed.FindAsync(tooaegaId);
            if (workHour == null)
            {
                return NotFound("Töötundi ei leitud.");
            }

            _context.IgapaevaAndmed.Remove(workHour);
            await _context.SaveChangesAsync();

            return Ok("Töötund edukalt kustutatud.");
        }

    }
}
