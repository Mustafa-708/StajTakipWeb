using System.Security.Claims;
namespace StajTakipWeb.Models;
public record SayfaBaglantisi(string Baslik, string Controller, string Action, string Isaret);
public class SayfaGezinme
{
    public string Rol { get; init; } = "";
    public string Baslik { get; init; } = "";
    public List<SayfaBaglantisi> Baglantilar { get; init; } = new();
    public SayfaBaglantisi? Aktif { get; init; }
    public static SayfaGezinme Olustur(ClaimsPrincipal user, string controller, string action, string baslik)
    {
        var links = new List<SayfaBaglantisi>(); var rol = "Hesap";
        if (user.IsInRole("Yonetici")) {
            rol = "Yönetici";
            links.AddRange(new[] {
                new SayfaBaglantisi("Genel bakış ve ödevler", "Yonetim", "Index", "▦"),
                new SayfaBaglantisi("Öğrenciler", "Yonetim", "Ogrenciler", "◎"),
                new SayfaBaglantisi("Ödev ata", "Yonetim", "OdevAta", "+"),
                new SayfaBaglantisi("Duyurular", "Yonetim", "Duyurular", "◈") });
        } else if (user.IsInRole("StajSorumlusu")) {
            rol = "Staj sorumlusu"; links.Add(new("Sorumlu paneli", "Panel", "StajSorumlusu", "▦"));
        } else if (user.IsInRole("Ogrenci")) {
            rol = "Öğrenci";
            links.AddRange(new[] {
                new SayfaBaglantisi("Genel bakış", "Panel", "Ogrenci", "▦"),
                new SayfaBaglantisi("Ödevlerim", "OgrenciPanel", "Odevler", "☑"),
                new SayfaBaglantisi("Teslim takvimim", "OgrenciPanel", "Takvim", "▤"),
                new SayfaBaglantisi("Staj defterim", "OgrenciPanel", "Defter", "▥"),
                new SayfaBaglantisi("Değerlendirmelerim", "OgrenciPanel", "Degerlendirmeler", "✓"),
                new SayfaBaglantisi("Duyurular", "OgrenciPanel", "Duyurular", "◈"),
                new SayfaBaglantisi("Staj bilgilerim", "OgrenciPanel", "Staj", "◇"),
                new SayfaBaglantisi("Kişisel bilgilerim", "OgrenciPanel", "Profil", "◎") });
        }
        links.Add(new("Şifre değiştir", "Account", "SifreDegistir", "⚿"));
        var parentAction = (controller, action) switch {
            ("Yonetim", "OgrenciDetay") => "Ogrenciler", ("Yonetim", "Incele") => "Index",
            ("OgrenciPanel", "Odev") => "Odevler", ("OgrenciPanel", "Rapor") => "Defter", _ => action };
        return new SayfaGezinme { Rol = rol, Baslik = baslik, Baglantilar = links, Aktif = links.FirstOrDefault(x => x.Controller == controller && x.Action == parentAction) };
    }
}
