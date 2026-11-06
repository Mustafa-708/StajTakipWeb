namespace StajTakipWeb.Models;

public static class OdevFiltresi
{
    public static IEnumerable<Odev> Uygula(IEnumerable<Odev> odevler, string? durum, string? ara)
    {
        if (!string.IsNullOrWhiteSpace(ara))
            odevler = odevler.Where(x => x.Baslik.Contains(ara, StringComparison.OrdinalIgnoreCase)
                || $"{x.Ogrenci?.Ad} {x.Ogrenci?.Soyad} {x.Ogrenci?.Numara}".Contains(ara, StringComparison.OrdinalIgnoreCase));
        return durum switch {
            "bekliyor" => odevler.Where(x => x.TeslimTarihi == null && x.SonTeslim.Date >= DateTime.Today),
            "geciken" => odevler.Where(x => x.TeslimTarihi == null && x.SonTeslim.Date < DateTime.Today),
            "teslim" => odevler.Where(x => x.TeslimTarihi != null && x.Not == null),
            "degerlendirilen" => odevler.Where(x => x.Not != null),
            _ => odevler
        };
    }
}
