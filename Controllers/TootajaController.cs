using API_XML_XSLT.Models;
using Microsoft.AspNetCore.Mvc;

namespace API_XML_XSLT.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TootajaController : Controller
    {
        private readonly HttpClient _httpClient;
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public List<Tootaja> Get()
        {
            return _tootaja;
        }
    }
}
