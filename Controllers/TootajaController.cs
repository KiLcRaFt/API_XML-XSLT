using API_XML_XSLT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_XML_XSLT.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TootajaController : Controller
    {
        private readonly TootajaDbContext _tootaja;
        //private readonly HttpClient _httpClient;

        public TootajaController(TootajaDbContext context)
        {
            _tootaja = context;
        }

        // GET: api/tootaja
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tootaja>>> GetTootaja()
        {
            var tootaja = await _tootaja.Tootajad.ToListAsync();
            return Ok(tootaja);
        }

        // GET: api/tootaja/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Tootaja>> GetTootaja(int id)
        {
            var tootaja = await _tootaja.Tootajad.FindAsync(id);

            if (tootaja == null)
            {
                return NotFound();
            }

            return Ok(tootaja);
        }

        // POST: api/tootaja
        [HttpPost]
        public async Task<ActionResult<Tootaja>> PostTootaja(Tootaja tootaja)
        {
            _tootaja.Tootajad.Add(tootaja);
            await _tootaja.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTootaja), new { id = tootaja.Id }, tootaja);
        }

        // DELETE: api/tootaja/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTootaja(int id)
        {
            var tootaja = await _tootaja.Tootajad.FindAsync(id);
            if (tootaja == null)
            {
                return NotFound();
            }

            _tootaja.Tootajad.Remove(tootaja);
            await _tootaja.SaveChangesAsync();

            return NoContent();
        }
    }
}
