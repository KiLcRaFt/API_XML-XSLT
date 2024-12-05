using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_XML_XSLT.Models;
using System.Linq;
using System.Threading.Tasks;

namespace API_XML_XSLT.Controllers
{
    [ApiController]
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
            var workHours = await _context.IgapaevaAndmed
                                          .Include(wh => wh.Tootaja) // Загрузка данных о сотрудниках
                                          .ToListAsync();

            if (workHours == null || !workHours.Any())
            {
                return NotFound("Töötундide andmeid ei leitud.");
            }

            var result = workHours.Select(wh => new
            {
                Id = wh.Id,
                TootajaId = wh.TootajaId,
                TootajaNimi = wh.Tootaja != null ? $"{wh.Tootaja.Nimi} {wh.Tootaja.Perenimi}" : "Неизвестный сотрудник",
                Kuupaev = wh.Kuupaev.ToString("yyyy-MM-dd"), // Преобразование даты в строку
                TooAlgus = wh.Too_algus.HasValue ? wh.Too_algus.Value.ToString(@"hh\:mm\:ss") : "N/A",
                TooLypp = wh.Too_lypp.HasValue ? wh.Too_lypp.Value.ToString(@"hh\:mm\:ss") : "N/A"
            });

            return Ok(result);
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

        [HttpGet("tooaeg/by_user")]
        public async Task<IActionResult> GetWorkHoursByUser(int tootajaId)
        {
            var workHours = await _context.IgapaevaAndmed.Where(w => w.TootajaId == tootajaId).ToListAsync();
            return Ok(workHours);
        }

        // POST: /admin/tooaeg_lisamine
        // Lisamine tööaeg töötajatele id-ga
        [HttpPost("tooaeg_lisamine")]
        public async Task<IActionResult> AddWorkHour([FromBody] TooAeg_andmed_id request)
        {
            // Проверка, что TootajaId передан в теле запроса
            int tootajaId = request.TootajaId;
            if (tootajaId <= 0)
            {
                return Unauthorized("TootajaId puudub või on vale.");
            }

            // Поиск пользователя по TootajaId
            var user = await _context.Tootajad.FindAsync(tootajaId);
            if (user == null)
            {
                return Unauthorized("Kasutajat ei leitud.");
            }

            // Инициализация значений времени
            TimeSpan tooAlgus = TimeSpan.Zero;
            TimeSpan tooLypp = TimeSpan.Zero;

            if (!string.IsNullOrEmpty(request.TooAlgus))
            {
                try
                {
                    tooAlgus = TimeSpan.Parse(request.TooAlgus); // Преобразование строки в TimeSpan
                }
                catch (FormatException)
                {
                    return BadRequest("Неверный формат времени для TooAlgus.");
                }
            }

            if (!string.IsNullOrEmpty(request.TooLypp))
            {
                try
                {
                    tooLypp = TimeSpan.Parse(request.TooLypp); // Преобразование строки в TimeSpan
                }
                catch (FormatException)
                {
                    return BadRequest("Неверный формат времени для TooLypp.");
                }
            }

            // Проверка даты работы (Kuupaev)
            if (request.Kuupaev == default)
            {
                return BadRequest("Kuupaev puudub või on vale.");
            }

            // Создание записи о рабочем времени
            var newWorkHour = new Igapaeva_andmed
            {
                TootajaId = tootajaId,
                Kuupaev = request.Kuupaev,
                Too_algus = tooAlgus,
                Too_lypp = tooLypp
            };

            // Сохранение записи в базе данных
            _context.IgapaevaAndmed.Add(newWorkHour);
            await _context.SaveChangesAsync();

            // Возвращаем успешный ответ с добавленной записью
            return Ok(newWorkHour);
        }

        // PUT: /admin/tooaeg_muudamine
        // Muudamine tööaega andmed töötajast id-ga
        [HttpPut("tooaeg_muudamine")]
        public async Task<IActionResult> UpdateWorkHour([FromBody] WorkHourUpdateModel model)
        {

            if (model == null)
            {
                return BadRequest("Invalid data.");
            }

            var existingWorkHour = await _context.IgapaevaAndmed.FindAsync(model.TooAegaId);
            if (existingWorkHour == null)
            {
                return NotFound($"Tööaeg ei ole leitud. ID: {model.TooAegaId}");
            }

            existingWorkHour.Kuupaev = model.Kuupaev;
            existingWorkHour.Too_algus = model.TooAlgus;
            existingWorkHour.Too_lypp = model.TooLypp;

            await _context.SaveChangesAsync();

            return Ok("Töötund edukalt uuendatud.");
        }

        // DELETE: /admin/tooaeg_kustutamine
        // Kustutamine konkreetselt tööaeg töötajast id-ga
        [HttpDelete("tooaeg_kustutamine")]
        public async Task<IActionResult> DeleteWorkHour([FromQuery] int tooaegaId, [FromHeader] string UserId)
        {

            Console.WriteLine($"Received UserId: {UserId}");
            if (string.IsNullOrEmpty(UserId) || !int.TryParse(UserId, out int tootajaId) || tootajaId <= 0)
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

            return Ok(new { message = "Töötund edukalt kustutatud." });
        }
    }
}
