
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StajTakipWeb.Models
{
    [Table("Ogrenciler")]
    public class Ogrenci
    {
        [Key]
        public int OgrenciID { get; set; }

        [Required]
        public string Numara { get; set; } = "";

        [Required]
        public string Ad { get; set; } = "";

        [Required]
        public string Soyad { get; set; } = "";

        [Required]
        public string Bolum { get; set; } = "";

        public int? Sinif { get; set; }

        public string? Telefon { get; set; }

        public string? Eposta { get; set; }

        public DateTime KayitTarihi { get; set; }
    }
}
