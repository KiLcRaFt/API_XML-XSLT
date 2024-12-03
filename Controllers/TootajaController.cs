using API_XML_XSLT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_XML_XSLT.Controllers
{
    
    [Route("[controller]")]
    [AdminOnly] // Sellesed endpointid saab kastada ainult admin
    [ApiController]
    public class TootajaController : Controller
    {
        private readonly TootajaDbContext _tootaja;

        public TootajaController(TootajaDbContext context)
        {
            _tootaja = context;
        }

        // GET: /Tootaja
        // Saatmine kõik töötajad
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tootaja>>> GetTootaja()
        {
            var tootaja = await _tootaja.Tootajad.ToListAsync();
            return Ok(tootaja);
        }

        // GET: /Tootaja/{id}
        // Saatmine konkretselt töötaja id-ga
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

        // POST: /Tootaja/Tootaja_lisamine
        // Lisamine uue töötaja
        [HttpPost("Tootaja_lisamine")]
        public async Task<ActionResult<Tootaja>> PostTootaja(string nimi, string perenimi, string email, string telefoni_number, string salasyna, bool is_admin, string amet)
        {
            var tootaja = new Tootaja
            {
                Nimi = nimi,
                Perenimi = perenimi,
                Email = email,
                Telefoni_number = telefoni_number,
                Salasyna = salasyna,
                Is_admin = is_admin,
                Amet = amet
            };
            _tootaja.Tootajad.Add(tootaja);
            await _tootaja.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTootaja), new { id = tootaja.Id }, tootaja);
        }

        // PUT: /Tootaja/Tootaja_muudamine
        // Uuendamine konkreetselt töötaja id-ga
        [HttpPut("Tootaja_muudamine")]
        public async Task<IActionResult> UpdateTootaja(
            int tootajaId,
            string nimi,
            string perenimi,
            string email,
            string telefoni_number,
            string salasyna,
            bool is_admin,
            string amet)
        {
            var tootaja = await _tootaja.Tootajad.FindAsync(tootajaId);

            if (tootaja == null)
            {
                return NotFound("Töötajat ei leitud.");
            }

            tootaja.Nimi = nimi;
            tootaja.Perenimi = perenimi;
            tootaja.Email = email;
            tootaja.Telefoni_number = telefoni_number;
            tootaja.Salasyna = salasyna;
            tootaja.Is_admin = is_admin;
            tootaja.Amet = amet;

            try
            {
                await _tootaja.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_tootaja.Tootajad.Any(e => e.Id == tootajaId))
                {
                    return NotFound("Töötajat ei eksisteeri.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: /Tootaja/Tootaja_kustutamine/{id}
        // Kustutamine konkretselt töötaja id-ga
        [HttpDelete("Tootaja_kustutamine")]
        public async Task<IActionResult> DeleteTootaja(int tootajaId)
        {
            var tootaja = await _tootaja.Tootajad.FindAsync(tootajaId);
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
