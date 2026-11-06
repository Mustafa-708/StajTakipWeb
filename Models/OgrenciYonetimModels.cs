using System.ComponentModel.DataAnnotations;
namespace StajTakipWeb.Models;
public class OgrenciYonetimSatiri
{
    public Ogrenci Ogrenci { get; set; } = new();
    public ApplicationUser? Hesap { get; set; }
}
public class OgrenciDetayModel : OgrenciYonetimSatiri
{
    public StajBilgisi? Staj { get; set; }
    public List<StajRaporu> Raporlar { get; set; } = new();
    public List<Odev> Odevler { get; set; } = new();
}
public class HesapDurumForm
{
    public bool AktifMi { get; set; }
    [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
    public string? Aciklama { get; set; }
}
public class RaporKararForm
{
    [Required] public string Durum { get; set; } = "";
    [Required(ErrorMessage = "Geri bildirim girin."), StringLength(4000)] public string GeriBildirim { get; set; } = "";
}
