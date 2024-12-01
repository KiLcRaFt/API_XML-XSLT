using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_XML_XSLT.Models;
using System.Linq;
using System.Threading.Tasks;

namespace API_XML_XSLT.Controllers
{
    [ApiController]
    [AdminOnly]
    [Route("api/admin")]
    public class AdminPanelController : ControllerBase
    {
        private readonly TootajaDbContext _context;

        public AdminPanelController(TootajaDbContext context)
        {
            _context = context;
        }

        // Получить все рабочие часы работников
        [HttpGet("work-hours")]
        public async Task<IActionResult> GetAllWorkHours()
        {
            var workHours = await _context.IgapaevaAndmed.ToListAsync();

            if (workHours == null || !workHours.Any())
            {
                return NotFound("No work hours data found.");
            }

            return Ok(workHours);
        }

        // Получить всех работников
        [HttpGet("workers")]
        public async Task<IActionResult> GetAllWorkers()
        {
            var workers = await _context.Tootajad.ToListAsync();

            if (workers == null || !workers.Any())
            {
                return NotFound("No workers found.");
            }

            return Ok(workers);
        }

        // Добавить новый рабочий час
        [HttpPost("add-work-hour")]
        public async Task<IActionResult> AddWorkHour([FromBody] Igapaeva_andmed newWorkHour)
        {
            if (newWorkHour == null)
            {
                return BadRequest("Invalid data.");
            }

            _context.IgapaevaAndmed.Add(newWorkHour);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAllWorkHours), new { id = newWorkHour.Id }, newWorkHour);
        }

        // Обновить рабочие часы
        [HttpPut("update-work-hour/{id}")]
        public async Task<IActionResult> UpdateWorkHour(int id, [FromBody] Igapaeva_andmed updatedWorkHour)
        {
            var existingWorkHour = await _context.IgapaevaAndmed.FindAsync(id);

            if (existingWorkHour == null)
            {
                return NotFound("Work hour not found.");
            }

            // Обновляем рабочие часы
            existingWorkHour.TootajaId = updatedWorkHour.TootajaId;
            existingWorkHour.Kuupaev = updatedWorkHour.Kuupaev;
            existingWorkHour.Too_algus = updatedWorkHour.Too_algus;
            existingWorkHour.Too_lypp = updatedWorkHour.Too_lypp;

            await _context.SaveChangesAsync();

            return Ok("Work hour updated successfully.");
        }

        // Удалить рабочий час
        [HttpDelete("delete-work-hour/{id}")]
        public async Task<IActionResult> DeleteWorkHour(int id)
        {
            var workHour = await _context.IgapaevaAndmed.FindAsync(id);

            if (workHour == null)
            {
                return NotFound("Work hour not found.");
            }

            _context.IgapaevaAndmed.Remove(workHour);
            await _context.SaveChangesAsync();

            return Ok("Work hour deleted successfully.");
        }
    }
}
