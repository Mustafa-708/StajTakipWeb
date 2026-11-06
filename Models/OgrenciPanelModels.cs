using System.ComponentModel.DataAnnotations;

namespace StajTakipWeb.Models;

public class ProfilForm
{
    [Required(ErrorMessage = "Telefon numaranızı girin."), Phone, StringLength(30)]
    [Display(Name = "Telefon")] public string? Telefon { get; set; }
    public Ogrenci? Ogrenci { get; set; }
    public string? Eposta { get; set; }
}

public class StajBilgisi : IValidatableObject
{
    public int Id { get; set; }
    public int OgrenciID { get; set; }
    [Required(ErrorMessage = "Firma adını girin."), StringLength(200), Display(Name = "Firma adı")]
    public string FirmaAdi { get; set; } = "";
    [Required, StringLength(500), Display(Name = "Firma adresi")]
    public string FirmaAdresi { get; set; } = "";
    [Required, StringLength(150), Display(Name = "İşyeri sorumlusu")]
    public string SorumluAdi { get; set; } = "";
    [Required, EmailAddress, StringLength(200), Display(Name = "Sorumlu e-posta")]
    public string SorumluEposta { get; set; } = "";
    [Required, DataType(DataType.Date), Display(Name = "Başlangıç tarihi")]
    public DateTime? Baslangic { get; set; }
    [Required, DataType(DataType.Date), Display(Name = "Bitiş tarihi")]
    public DateTime? Bitis { get; set; }
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (Bitis < Baslangic)
            yield return new ValidationResult("Bitiş tarihi başlangıçtan önce olamaz.", new[] { nameof(Bitis) });
    }
}

public class StajRaporu
{
    public int Id { get; set; }
    public int OgrenciID { get; set; }
    [Required, DataType(DataType.Date), Display(Name = "Çalışma tarihi")]
    public DateTime? Tarih { get; set; }
    [Required(ErrorMessage = "Rapor başlığını girin."), StringLength(150), Display(Name = "Başlık")]
    public string Baslik { get; set; } = "";
    [Required(ErrorMessage = "Yaptığınız çalışmaları yazın."), StringLength(10000), Display(Name = "Yapılan çalışmalar")]
    public string Icerik { get; set; } = "";
    public string Durum { get; set; } = "Taslak";
    public string? Degerlendirme { get; set; }
}

public class OgrenciPanelModel
{
    public Ogrenci Ogrenci { get; set; } = new();
    public StajBilgisi? Staj { get; set; }
    public List<StajRaporu> Raporlar { get; set; } = new();
    public List<Odev> Odevler { get; set; } = new();
    public int OkunmamisDuyuruSayisi { get; set; }
}
