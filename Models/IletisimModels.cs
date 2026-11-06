using System.ComponentModel.DataAnnotations;
namespace StajTakipWeb.Models;

public class Duyuru
{
    public int Id { get; set; }
    [Required, StringLength(150)] public string Baslik { get; set; } = "";
    [Required, StringLength(5000)] public string Icerik { get; set; } = "";
    public DateTime YayinTarihi { get; set; }
    public DateTime? Bitis { get; set; }
    public bool YayindaMi { get; set; } = true;
}
public class DuyuruOkuma
{
    public int DuyuruId { get; set; }
    public string KullaniciId { get; set; } = "";
    public DateTime OkumaTarihi { get; set; }
}
public class DuyuruForm
{
    [Required(ErrorMessage = "Duyuru başlığını girin."), StringLength(150), Display(Name = "Başlık")] public string Baslik { get; set; } = "";
    [Required(ErrorMessage = "Duyuru metnini girin."), StringLength(5000), Display(Name = "Duyuru metni")] public string Icerik { get; set; } = "";
    [DataType(DataType.Date), Display(Name = "Son gösterim günü (isteğe bağlı)")] public DateTime? Bitis { get; set; }
    public List<Duyuru> Kayitlar { get; set; } = new();
}
public class DuyuruSatiri
{
    public Duyuru Duyuru { get; set; } = new();
    public bool Okundu { get; set; }
}
public class SifreDegistirForm
{
    [Required(ErrorMessage = "Mevcut şifrenizi girin."), DataType(DataType.Password), Display(Name = "Mevcut şifre")] public string MevcutSifre { get; set; } = "";
    [Required(ErrorMessage = "Yeni şifrenizi girin."), StringLength(100, MinimumLength = 8, ErrorMessage = "Şifre 8–100 karakter olmalıdır."), DataType(DataType.Password), Display(Name = "Yeni şifre")] public string YeniSifre { get; set; } = "";
    [Required, Compare(nameof(YeniSifre), ErrorMessage = "Yeni şifreler eşleşmiyor."), DataType(DataType.Password), Display(Name = "Yeni şifre tekrarı")] public string YeniSifreTekrar { get; set; } = "";
}
public class TakvimModel
{
    public DateTime Ay { get; set; }
    public List<Odev> Odevler { get; set; } = new();
    public int BaslangicBosluk => ((int)Ay.DayOfWeek + 6) % 7;
    public int GunSayisi => DateTime.DaysInMonth(Ay.Year, Ay.Month);
}
