using System.ComponentModel.DataAnnotations;
namespace StajTakipWeb.Models;

public class Odev
{
    public int Id { get; set; }
    public int OgrenciID { get; set; }
    public Ogrenci? Ogrenci { get; set; }
    public string AtayanId { get; set; } = "";
    [Required, StringLength(150)] public string Baslik { get; set; } = "";
    [Required, StringLength(10000)] public string Aciklama { get; set; } = "";
    public DateTime SonTeslim { get; set; }
    public DateTime Olusturma { get; set; }
    public string? TeslimMetni { get; set; }
    public string? TeslimBaglantisi { get; set; }
    public string? TeslimDosyasi { get; set; }
    public DateTime? TeslimTarihi { get; set; }
    public int? Not { get; set; }
    public string? GeriBildirim { get; set; }
    public string Durum => Not.HasValue ? "Değerlendirildi" : TeslimTarihi.HasValue ? "Teslim edildi" : SonTeslim.Date < DateTime.Today ? "Süresi geçti" : "Bekliyor";
}
public class OdevForm
{
    public List<int> OgrenciIDleri { get; set; } = new();
    public bool TumOgrenciler { get; set; }
    [Required(ErrorMessage = "Başlık girin."), StringLength(150), Display(Name = "Ödev başlığı")] public string Baslik { get; set; } = "";
    [Required(ErrorMessage = "Açıklama girin."), StringLength(10000), Display(Name = "Açıklama ve beklentiler")] public string Aciklama { get; set; } = "";
    [Required, DataType(DataType.Date), Display(Name = "Son teslim tarihi")] public DateTime? SonTeslim { get; set; }
    public List<Ogrenci> Ogrenciler { get; set; } = new();
}
public class TeslimForm
{
    [Required(ErrorMessage = "Yaptığınız çalışmayı açıklayın."), StringLength(10000), Display(Name = "Çalışma açıklaması")] public string TeslimMetni { get; set; } = "";
    [StringLength(2000), Display(Name = "Çalışma bağlantısı (isteğe bağlı)")] public string? TeslimBaglantisi { get; set; }
    [Display(Name = "Dosya (isteğe bağlı)")] public IFormFile? TeslimDosyasi { get; set; }
    public Odev? Odev { get; set; }
}
public class OdevIncelemeForm
{
    [Required, Range(0, 100), Display(Name = "Not (0–100)")] public int? Not { get; set; }
    [Required(ErrorMessage = "Geri bildirim girin."), StringLength(4000), Display(Name = "Geri bildirim")] public string GeriBildirim { get; set; } = "";
    public Odev? Odev { get; set; }
}
public class YoneticiPanelModel
{
    public int OgrenciSayisi { get; set; }
    public List<Odev> Odevler { get; set; } = new();
    public List<Odev> FiltrelenenOdevler { get; set; } = new();
    public List<StajRaporu> BekleyenRaporlar { get; set; } = new();
    public int PasifHesapSayisi { get; set; }
}
