
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajTakipWeb.Data;
using StajTakipWeb.Models;

namespace StajTakipWeb.Controllers
{
    // Öğrenci yönetimine sadece yetkililer erişebilir.
    [Authorize(Roles = "Yonetici,StajSorumlusu")]
    public class OgrencilerController : Controller
    {
        private readonly StajTakipContext _context;

        // SQL Server bağlantısını alır.
        public OgrencilerController(StajTakipContext context)
        {
            _context = context;
        }

        // =====================================
        // 1. ÖĞRENCİ LİSTESİ
        // GET: /Ogrenciler
        // =====================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var ogrenciler = await _context.Ogrenciler
                .AsNoTracking()
                .OrderBy(o => o.OgrenciID)
                .ToListAsync();

            return View(ogrenciler);
        }

        // =====================================
        // 2. ÖĞRENCİ EKLEME SAYFASI
        // GET: /Ogrenciler/Create
        // =====================================

        [Authorize(Roles = "Yonetici")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // =====================================
        // 3. ÖĞRENCİYİ VERİTABANINA KAYDET
        // POST: /Ogrenciler/Create
        // =====================================

        [Authorize(Roles = "Yonetici")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ogrenci ogrenci)
        {
            // Aynı öğrenci numarası var mı?
            if (await _context.Ogrenciler.AnyAsync(
                x => x.Numara == ogrenci.Numara))
            {
                ModelState.AddModelError(
                    "Numara",
                    "Bu öğrenci numarası zaten kayıtlı.");
            }

            // Formda hata varsa tekrar göster.
            if (!ModelState.IsValid)
            {
                return View(ogrenci);
            }

            // Kayıt tarihi
            ogrenci.KayitTarihi = DateTime.Now;

            // Öğrenciyi veritabanına ekle.
            _context.Ogrenciler.Add(ogrenci);

            await _context.SaveChangesAsync();

            // Öğrenci listesine geri dön.
            return RedirectToAction(nameof(Index));
        }
    }
}
