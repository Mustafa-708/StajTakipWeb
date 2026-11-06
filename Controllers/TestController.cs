
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajTakipWeb.Data;

namespace StajTakipWeb.Controllers
{
    public class TestController : Controller
    {
        private readonly StajTakipContext _context;

        public TestController(StajTakipContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            bool baglanti = await _context.Database.CanConnectAsync();

            if (baglanti)
            {
                return Content(
                    "SQL Server bağlantısı başarılı!");
            }

            return Content(
                "SQL Server bağlantısı kurulamadı.");
        }
    }
}
