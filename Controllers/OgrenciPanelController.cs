using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajTakipWeb.Data;
using StajTakipWeb.Models;

namespace StajTakipWeb.Controllers;

[Authorize(Roles = "Ogrenci")]
public class OgrenciPanelController(StajTakipContext db, UserManager<ApplicationUser> users) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Duyurular()
    {
        var userId = users.GetUserId(User);
        var bugun = DateTime.Today;
        return View(await db.Duyurular.AsNoTracking().Where(x => x.YayindaMi && (x.Bitis == null || x.Bitis >= bugun))
            .OrderByDescending(x => x.YayinTarihi).Select(x => new DuyuruSatiri { Duyuru = x, Okundu = db.DuyuruOkumalari.Any(r => r.DuyuruId == x.Id && r.KullaniciId == userId) }).ToListAsync());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DuyuruOku(int id)
    {
        var user = await users.GetUserAsync(User);
        if (user == null) return NotFound();
        var bugun = DateTime.Today;
        if (!await db.Duyurular.AnyAsync(x => x.Id == id && x.YayindaMi && (x.Bitis == null || x.Bitis >= bugun))) return NotFound();
        // Tekrarlanan tıklamalarda birleşik anahtar çakışmasını önlemek için atomik ekleme.
        await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO [dbo].[DuyuruOkumalari] ([DuyuruId], [KullaniciId], [OkumaTarihi]) SELECT {id}, {user.Id}, {DateTime.Now} WHERE NOT EXISTS (SELECT 1 FROM [dbo].[DuyuruOkumalari] WITH (UPDLOCK, HOLDLOCK) WHERE [DuyuruId] = {id} AND [KullaniciId] = {user.Id})");
        TempData["SuccessMessage"] = "Duyuru okundu olarak işaretlendi.";
        return RedirectToAction(nameof(Duyurular));
    }

    [HttpGet]
    public async Task<IActionResult> Takvim(int? yil, int? ay)
    {
        var ogrenci = await OgrenciAsync();
        if (ogrenci == null) return NotFound();
        var year = yil ?? DateTime.Today.Year; var month = ay ?? DateTime.Today.Month;
        if (year < 2000 || year > 2100 || month < 1 || month > 12) return BadRequest("Geçerli bir ay ve yıl seçin.");
        var baslangic = new DateTime(year, month, 1); var bitis = baslangic.AddMonths(1);
        return View(new TakvimModel { Ay = baslangic, Odevler = await db.Odevler.AsNoTracking().Where(x => x.OgrenciID == ogrenci.OgrenciID && x.SonTeslim >= baslangic && x.SonTeslim < bitis).OrderBy(x => x.SonTeslim).ThenBy(x => x.Baslik).ToListAsync() });
    }

    private async Task<Ogrenci?> OgrenciAsync()
    {
        var user = await users.GetUserAsync(User);
        return user?.OgrenciID is int id ? await db.Ogrenciler.FindAsync(id) : null;
    }

    [HttpGet]
    public async Task<IActionResult> Odevler(string? durum, string? ara)
    {
        var ogrenci = await OgrenciAsync();
        if (ogrenci == null) return NotFound();
        ViewData["Durum"] = durum; ViewData["Ara"] = ara;
        var odevler = await db.Odevler.AsNoTracking().Where(x => x.OgrenciID == ogrenci.OgrenciID).OrderBy(x => x.SonTeslim).ToListAsync();
        return View(OdevFiltresi.Uygula(odevler, durum, ara).ToList());
    }

    [HttpGet]
    public async Task<IActionResult> Odev(int id)
    {
        var ogrenci = await OgrenciAsync();
        if (ogrenci == null) return NotFound();
        var odev = await db.Odevler.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && x.OgrenciID == ogrenci.OgrenciID);
        return odev == null ? NotFound() : View(new TeslimForm { Odev = odev });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Odev(int id, [Bind("TeslimMetni,TeslimBaglantisi")] TeslimForm form)
    {
        var ogrenci = await OgrenciAsync();
        if (ogrenci == null) return NotFound();
        var odev = await db.Odevler.SingleOrDefaultAsync(x => x.Id == id && x.OgrenciID == ogrenci.OgrenciID);
        if (odev == null) return NotFound();
        if (odev.TeslimTarihi != null) return BadRequest("Bu ödev zaten teslim edildi.");
        if (odev.SonTeslim.Date < DateTime.Today) ModelState.AddModelError("", "Ödevin teslim süresi doldu.");
        if (!string.IsNullOrWhiteSpace(form.TeslimBaglantisi) && (!Uri.TryCreate(form.TeslimBaglantisi, UriKind.Absolute, out var uri) || (uri.Scheme != "http" && uri.Scheme != "https"))) ModelState.AddModelError("TeslimBaglantisi", "http veya https ile başlayan geçerli bir bağlantı girin.");
        if (!ModelState.IsValid) { form.Odev = odev; return View(form); }
        odev.TeslimMetni = form.TeslimMetni;
        odev.TeslimBaglantisi = form.TeslimBaglantisi;
        odev.TeslimTarihi = DateTime.Now;
        await db.SaveChangesAsync();
        TempData["SuccessMessage"] = "Ödeviniz teslim edildi.";
        return RedirectToAction(nameof(Odev), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Profil()
    {
        var ogrenci = await OgrenciAsync();
        if (ogrenci == null) return NotFound("Hesabınıza bağlı öğrenci kaydı bulunamadı.");
        return View(new ProfilForm { Ogrenci = ogrenci, Telefon = ogrenci.Telefon, Eposta = (await users.GetUserAsync(User))?.Email });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Profil([Bind("Telefon")] ProfilForm form)
    {
        var ogrenci = await OgrenciAsync();
        if (ogrenci == null) return NotFound();
        if (ModelState.IsValid)
        {
            ogrenci.Telefon = form.Telefon;
            await db.SaveChangesAsync();
            TempData["SuccessMessage"] = "İletişim bilgileriniz güncellendi.";
            return RedirectToAction(nameof(Profil));
        }
        form.Ogrenci = ogrenci;
        form.Eposta = (await users.GetUserAsync(User))?.Email;
        return View(form);
    }

    [HttpGet]
    public async Task<IActionResult> Staj()
    {
        var ogrenci = await OgrenciAsync();
        if (ogrenci == null) return NotFound();
        return View(await db.StajBilgileri.SingleOrDefaultAsync(x => x.OgrenciID == ogrenci.OgrenciID) ?? new StajBilgisi());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Staj([Bind("FirmaAdi,FirmaAdresi,SorumluAdi,SorumluEposta,Baslangic,Bitis")] StajBilgisi form)
    {
        var ogrenci = await OgrenciAsync();
        if (ogrenci == null) return NotFound();
        if (form.Baslangic.HasValue && form.Bitis.HasValue && await db.StajRaporlari.AnyAsync(x => x.OgrenciID == ogrenci.OgrenciID && (x.Tarih < form.Baslangic || x.Tarih > form.Bitis)))
            ModelState.AddModelError("", "Yeni tarih aralığı mevcut raporlarınızın tarihlerini kapsamalıdır.");
        if (!ModelState.IsValid) return View(form);
        var staj = await db.StajBilgileri.SingleOrDefaultAsync(x => x.OgrenciID == ogrenci.OgrenciID);
        if (staj == null) { staj = new StajBilgisi { OgrenciID = ogrenci.OgrenciID }; db.Add(staj); }
        staj.FirmaAdi = form.FirmaAdi; staj.FirmaAdresi = form.FirmaAdresi;
        staj.SorumluAdi = form.SorumluAdi; staj.SorumluEposta = form.SorumluEposta;
        staj.Baslangic = form.Baslangic; staj.Bitis = form.Bitis;
        await db.SaveChangesAsync();
        TempData["SuccessMessage"] = "Staj bilgileriniz kaydedildi.";
        return RedirectToAction(nameof(Staj));
    }

    [HttpGet]
    public async Task<IActionResult> Defter()
    {
        var ogrenci = await OgrenciAsync();
        if (ogrenci == null) return NotFound();
        return View(await db.StajRaporlari.Where(x => x.OgrenciID == ogrenci.OgrenciID).OrderByDescending(x => x.Tarih).ToListAsync());
    }

    [HttpGet]
    public async Task<IActionResult> Rapor(int? id)
    {
        var ogrenci = await OgrenciAsync();
        if (ogrenci == null) return NotFound();
        if (id == null) return View(new StajRaporu { Tarih = DateTime.Today });
        var rapor = await db.StajRaporlari.SingleOrDefaultAsync(x => x.Id == id && x.OgrenciID == ogrenci.OgrenciID);
        return rapor == null ? NotFound() : View(rapor);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Rapor(int? id, [Bind("Tarih,Baslik,Icerik")] StajRaporu form, string islem)
    {
        var ogrenci = await OgrenciAsync();
        if (ogrenci == null) return NotFound();
        var rapor = id.HasValue ? await db.StajRaporlari.SingleOrDefaultAsync(x => x.Id == id && x.OgrenciID == ogrenci.OgrenciID) : null;
        if (id.HasValue && rapor == null) return NotFound();
        if (rapor != null && rapor.Durum != "Taslak" && rapor.Durum != "Reddedildi") return BadRequest("Değerlendirme bekleyen veya onaylanmış rapor değiştirilemez.");
        var staj = await db.StajBilgileri.SingleOrDefaultAsync(x => x.OgrenciID == ogrenci.OgrenciID);
        if (staj == null) ModelState.AddModelError("", "Önce staj bilgilerinizi kaydedin.");
        else if (form.Tarih < staj.Baslangic || form.Tarih > staj.Bitis) ModelState.AddModelError("Tarih", "Tarih staj aralığında olmalıdır.");
        if (form.Tarih > DateTime.Today) ModelState.AddModelError("Tarih", "İleri bir tarih için rapor yazamazsınız.");
        if (await db.StajRaporlari.AnyAsync(x => x.OgrenciID == ogrenci.OgrenciID && x.Tarih == form.Tarih && x.Id != (id ?? 0)))
            ModelState.AddModelError("Tarih", "Bu tarih için zaten bir raporunuz var.");
        if (!ModelState.IsValid) { form.Id = id ?? 0; form.Durum = rapor?.Durum ?? "Taslak"; form.Degerlendirme = rapor?.Degerlendirme; return View(form); }
        if (rapor == null) { rapor = new StajRaporu { OgrenciID = ogrenci.OgrenciID }; db.Add(rapor); }
        rapor.Tarih = form.Tarih; rapor.Baslik = form.Baslik; rapor.Icerik = form.Icerik;
        rapor.Durum = islem == "gonder" ? "Onay bekliyor" : "Taslak";
        if (islem == "gonder") rapor.Degerlendirme = null;
        await db.SaveChangesAsync();
        TempData["SuccessMessage"] = islem == "gonder" ? "Raporunuz değerlendirmeye gönderildi." : "Raporunuz taslak olarak kaydedildi.";
        return RedirectToAction(nameof(Defter));
    }

    [HttpGet]
    public async Task<IActionResult> Degerlendirmeler()
    {
        var ogrenci = await OgrenciAsync();
        if (ogrenci == null) return NotFound();
        return View(await db.StajRaporlari.Where(x => x.OgrenciID == ogrenci.OgrenciID && x.Durum != "Taslak").OrderByDescending(x => x.Tarih).ToListAsync());
    }
}
