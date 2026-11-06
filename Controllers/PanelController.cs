
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StajTakipWeb.Data;
using StajTakipWeb.Models;

namespace StajTakipWeb.Controllers
{
    [Authorize]
    public class PanelController(StajTakipContext db, UserManager<ApplicationUser> users) : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            if (User.IsInRole("Yonetici"))
                return RedirectToAction(nameof(Yonetici));

            if (User.IsInRole("StajSorumlusu"))
                return RedirectToAction(nameof(StajSorumlusu));

            if (User.IsInRole("Ogrenci"))
                return RedirectToAction(nameof(Ogrenci));

            return RedirectToAction(
                "AccessDenied", "Account");
        }

        [Authorize(Roles = "Ogrenci")]
        public async Task<IActionResult> Ogrenci()
        {
            var user = await users.GetUserAsync(User);
            var ogrenci = user?.OgrenciID is int id ? await db.Ogrenciler.FindAsync(id) : null;
            if (ogrenci == null) return NotFound("Hesabınıza bağlı öğrenci kaydı bulunamadı.");
            return View(new OgrenciPanelModel {
                Ogrenci = ogrenci,
                OkunmamisDuyuruSayisi = await db.Duyurular.CountAsync(x => x.YayindaMi && (x.Bitis == null || x.Bitis >= DateTime.Today) && !db.DuyuruOkumalari.Any(r => r.DuyuruId == x.Id && r.KullaniciId == user!.Id)),
                Staj = await db.StajBilgileri.AsNoTracking().SingleOrDefaultAsync(x => x.OgrenciID == ogrenci.OgrenciID),
                Odevler = await db.Odevler.AsNoTracking().Where(x => x.OgrenciID == ogrenci.OgrenciID).OrderBy(x => x.SonTeslim).ToListAsync(),
                Raporlar = await db.StajRaporlari.AsNoTracking().Where(x => x.OgrenciID == ogrenci.OgrenciID).OrderByDescending(x => x.Tarih).ToListAsync()
            });
        }

        [Authorize(Roles = "StajSorumlusu")]
        public IActionResult StajSorumlusu()
        {
            return View();
        }

        [Authorize(Policy = "SingleAdmin")]
        public IActionResult Yonetici()
        {
            return RedirectToAction("Index", "Yonetim");
        }
    }
}
