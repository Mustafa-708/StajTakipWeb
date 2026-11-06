
using Microsoft.AspNetCore.Identity;

namespace StajTakipWeb.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Ad { get; set; } = "";
        public string Soyad { get; set; } = "";

        public int? OgrenciID { get; set; }
        public bool AktifMi { get; set; } = true;
        public DateTime? SonGiris { get; set; }
        public string? HesapNotu { get; set; }
    }
}
