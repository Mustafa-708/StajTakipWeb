using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajTakipWeb.Data;
using StajTakipWeb.Models;
namespace StajTakipWeb.Controllers;

[Authorize(Policy = "SingleAdmin")]
public class YonetimController(StajTakipContext db, UserManager<ApplicationUser> users) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Duyurular() => View(new DuyuruForm { Kayitlar = await db.Duyurular.AsNoTracking().OrderByDescending(x => x.YayinTarihi).ToListAsync() });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Duyurular([Bind("Baslik,Icerik,Bitis")] DuyuruForm form)
    {
        if (form.Bitis?.Date < DateTime.Today) ModelState.AddModelError("Bitis", "Son gösterim günü geçmişte olamaz.");
        if (!ModelState.IsValid) { form.Kayitlar = await db.Duyurular.AsNoTracking().OrderByDescending(x => x.YayinTarihi).ToListAsync(); return View(form); }
        db.Duyurular.Add(new Duyuru { Baslik = form.Baslik.Trim(), Icerik = form.Icerik.Trim(), Bitis = form.Bitis?.Date, YayinTarihi = DateTime.Now });
        await db.SaveChangesAsync();
        TempData["SuccessMessage"] = "Duyuru tüm öğrenciler için yayımlandı.";
        return RedirectToAction(nameof(Duyurular));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DuyuruYayini(int id, bool yayindaMi)
    {
        var duyuru = await db.Duyurular.FindAsync(id);
        if (duyuru == null) return NotFound();
        if (yayindaMi && duyuru.Bitis?.Date < DateTime.Today) { TempData["ErrorMessage"] = "Bu duyurunun gösterim süresi doldu. Yeni bir duyuru oluşturun."; return RedirectToAction(nameof(Duyurular)); }
        duyuru.YayindaMi = yayindaMi;
        await db.SaveChangesAsync();
        TempData["SuccessMessage"] = yayindaMi ? "Duyuru yeniden yayında." : "Duyuru arşive alındı.";
        return RedirectToAction(nameof(Duyurular));
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? durum, string? ara)
    {
        var odevler = await db.Odevler.Include(x => x.Ogrenci).AsNoTracking().OrderByDescending(x => x.Olusturma).ToListAsync();
        ViewData["Durum"] = durum; ViewData["Ara"] = ara;
        return View(new YoneticiPanelModel {
            OgrenciSayisi = await db.Ogrenciler.CountAsync(), Odevler = odevler,
            FiltrelenenOdevler = OdevFiltresi.Uygula(odevler, durum, ara).ToList(),
            BekleyenRaporlar = await db.StajRaporlari.AsNoTracking().Where(x => x.Durum == "Onay bekliyor").OrderBy(x => x.Tarih).ToListAsync(),
            PasifHesapSayisi = await db.Users.CountAsync(x => x.OgrenciID != null && !x.AktifMi)
        });
    }

    [HttpGet]
    public async Task<IActionResult> Ogrenciler(string? ara, string? durum)
    {
        var query = db.Ogrenciler.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(ara)) query = query.Where(x => x.Ad.Contains(ara) || x.Soyad.Contains(ara) || x.Numara.Contains(ara));
        ViewData["Ara"] = ara;
        ViewData["Durum"] = durum;
        var students = await query.OrderBy(x => x.Ad).ThenBy(x => x.Soyad).ToListAsync();
        var accounts = await db.Users.AsNoTracking().Where(x => x.OgrenciID != null).ToListAsync();
        var rows = students.Select(x => new OgrenciYonetimSatiri { Ogrenci = x, Hesap = accounts.FirstOrDefault(a => a.OgrenciID == x.OgrenciID) });
        if (durum == "aktif") rows = rows.Where(x => x.Hesap?.AktifMi == true);
        if (durum == "pasif") rows = rows.Where(x => x.Hesap?.AktifMi == false);
        return View(rows.ToList());
    }

    [HttpGet]
    public async Task<IActionResult> OgrenciDetay(int id)
    {
        var ogrenci = await db.Ogrenciler.AsNoTracking().SingleOrDefaultAsync(x => x.OgrenciID == id);
        if (ogrenci == null) return NotFound();
        return View(new OgrenciDetayModel {
            Ogrenci = ogrenci, Hesap = await db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.OgrenciID == id),
            Staj = await db.StajBilgileri.AsNoTracking().SingleOrDefaultAsync(x => x.OgrenciID == id),
            Raporlar = await db.StajRaporlari.AsNoTracking().Where(x => x.OgrenciID == id).OrderByDescending(x => x.Tarih).ToListAsync(),
            Odevler = await db.Odevler.AsNoTracking().Where(x => x.OgrenciID == id).OrderByDescending(x => x.Olusturma).ToListAsync()
        });
    }

    private async Task<ApplicationUser?> YonetilebilirHesap(int id)
    {
        var target = await db.Users.SingleOrDefaultAsync(x => x.OgrenciID == id);
        if (target == null || target.Id == users.GetUserId(User) ||
            await users.IsInRoleAsync(target, "Yonetici") || !await users.IsInRoleAsync(target, "Ogrenci")) return null;
        return target;
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> HesapDurumu(int id, HesapDurumForm form)
    {
        var target = await YonetilebilirHesap(id);
        if (target == null) return Forbid();
        if (!form.AktifMi && string.IsNullOrWhiteSpace(form.Aciklama)) ModelState.AddModelError("Aciklama", "Pasife alma nedenini yazın.");
        if (!ModelState.IsValid) { TempData["ErrorMessage"] = "Hesap durumu değişmedi. Açıklama 1–500 karakter olmalıdır."; return RedirectToAction(nameof(OgrenciDetay), new { id }); }
        target.AktifMi = form.AktifMi;
        target.HesapNotu = form.Aciklama?.Trim();
        // Durum ve oturum iptali aynı kullanıcı güncellemesinde kaydedilir.
        target.SecurityStamp = Guid.NewGuid().ToString();
        var result = await users.UpdateAsync(target);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Succeeded
            ? (form.AktifMi ? "Hesap aktifleştirildi. Öğrenci yeniden giriş yapabilir." : "Hesap pasife alındı. Açık oturumlar bir sonraki istekte kapatılacak.")
            : "Hesap güncellenemedi. Sayfayı yenileyip tekrar deneyin.";
        return RedirectToAction(nameof(OgrenciDetay), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> OturumKapat(int id)
    {
        var target = await YonetilebilirHesap(id);
        if (target == null) return Forbid();
        var result = await users.UpdateSecurityStampAsync(target);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Succeeded
            ? "Öğrencinin tüm oturumları bir sonraki istekte kapatılacak. Hesabı aktifse yeniden giriş yapabilir."
            : "Oturumlar kapatılamadı. Tekrar deneyin.";
        return RedirectToAction(nameof(OgrenciDetay), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RaporKarari(int id, RaporKararForm form)
    {
        var rapor = await db.StajRaporlari.FindAsync(id);
        if (rapor == null) return NotFound();
        if (rapor.Durum == "Taslak") return BadRequest("Taslak rapor değerlendirilemez.");
        if (form.Durum != "Onaylandı" && form.Durum != "Reddedildi") ModelState.AddModelError("Durum", "Geçersiz karar.");
        if (ModelState.IsValid) { rapor.Durum = form.Durum; rapor.Degerlendirme = form.GeriBildirim; await db.SaveChangesAsync(); TempData["SuccessMessage"] = "Rapor değerlendirmesi kaydedildi."; }
        else TempData["ErrorMessage"] = "Karar seçin ve en fazla 4000 karakterlik geri bildirim yazın.";
        return RedirectToAction(nameof(OgrenciDetay), new { id = rapor.OgrenciID });
    }

    [HttpGet]
    public async Task<IActionResult> OdevAta(int? ogrenciId) => View(new OdevForm {
        OgrenciIDleri = ogrenciId.HasValue ? new List<int> { ogrenciId.Value } : new(), SonTeslim = DateTime.Today.AddDays(7),
        Ogrenciler = await db.Ogrenciler.AsNoTracking().OrderBy(x => x.Ad).ToListAsync()
    });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> OdevAta([Bind("OgrenciIDleri,TumOgrenciler,Baslik,Aciklama,SonTeslim")] OdevForm form)
    {
        form.Ogrenciler = await db.Ogrenciler.AsNoTracking().OrderBy(x => x.Ad).ThenBy(x => x.Soyad).ToListAsync();
        var mevcutIdler = form.Ogrenciler.Select(x => x.OgrenciID).ToHashSet();
        var secilenIdler = form.TumOgrenciler ? mevcutIdler.ToList() : form.OgrenciIDleri.Distinct().ToList();
        if (secilenIdler.Count == 0) ModelState.AddModelError("OgrenciIDleri", "En az bir öğrenci seçin.");
        if (secilenIdler.Any(id => !mevcutIdler.Contains(id))) ModelState.AddModelError("OgrenciIDleri", "Seçilen öğrencilerden biri bulunamadı. Listeyi kontrol edin.");
        if (form.SonTeslim?.Date < DateTime.Today) ModelState.AddModelError("SonTeslim", "Geçmiş bir teslim tarihi seçemezsiniz.");
        if (!ModelState.IsValid) return View(form);
        var atayanId = users.GetUserId(User)!;
        var olusturma = DateTime.Now;
        db.Odevler.AddRange(secilenIdler.Select(id => new Odev { OgrenciID = id, Baslik = form.Baslik, Aciklama = form.Aciklama, SonTeslim = form.SonTeslim!.Value.Date, Olusturma = olusturma, AtayanId = atayanId }));
        await db.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Ödev {secilenIdler.Count} öğrenciye atandı.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Incele(int id)
    {
        var odev = await db.Odevler.Include(x => x.Ogrenci).AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
        return odev == null ? NotFound() : View(new OdevIncelemeForm { Odev = odev, Not = odev.Not, GeriBildirim = odev.GeriBildirim ?? "" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Incele(int id, [Bind("Not,GeriBildirim")] OdevIncelemeForm form)
    {
        var odev = await db.Odevler.Include(x => x.Ogrenci).SingleOrDefaultAsync(x => x.Id == id);
        if (odev == null) return NotFound();
        if (odev.TeslimTarihi == null) return BadRequest("Henüz teslim edilmemiş ödev değerlendirilemez.");
        if (!ModelState.IsValid) { form.Odev = odev; return View(form); }
        odev.Not = form.Not; odev.GeriBildirim = form.GeriBildirim;
        await db.SaveChangesAsync();
        TempData["SuccessMessage"] = "Değerlendirme kaydedildi.";
        return RedirectToAction(nameof(Incele), new { id });
    }
}
