
using System.ComponentModel.DataAnnotations;

namespace StajTakipWeb.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Ad zorunludur.")]
        public string Ad { get; set; } = "";

        [Required(ErrorMessage = "Soyad zorunludur.")]
        public string Soyad { get; set; } = "";

        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        [Display(Name = "Kullanıcı Adı")]
        public string UserName { get; set; } = "";

        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Şifre tekrarı zorunludur.")]
        [DataType(DataType.Password)]
        [Compare("Password",
            ErrorMessage = "Şifreler eşleşmiyor.")]
        public string ConfirmPassword { get; set; } = "";

        [Required(ErrorMessage = "Öğrenci numarası zorunludur.")]
        [Display(Name = "Öğrenci Numarası")]
        public string Numara { get; set; } = "";

        [Required(ErrorMessage = "Bölüm zorunludur.")]
        [Display(Name = "Bölüm")]
        public string Bolum { get; set; } = "";

        [Required(ErrorMessage = "Sınıf zorunludur.")]
        [Range(9, 12, ErrorMessage = "Sınıf 9 ile 12 arasında olmalıdır.")]
        [Display(Name = "Sınıf")]
        public int? Sinif { get; set; }
    }
}

