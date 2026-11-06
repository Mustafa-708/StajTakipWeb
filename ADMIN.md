# Yönetici ve ödev paneli

- Yönetici girişi mevcut `/Account/Login` ekranından yapılır.
- Yönetici paneli: `/Yonetim/Index`.
- Öğrenci görevleri: `/OgrenciPanel/Odevler`.
- `SingleAdmin` politikası, veritabanında tam bir yönetici bulunmasını ve oturumun o hesaba ait olmasını zorunlu tutar.
- Web üzerinden yönetici rolü atanmaz. Kayıt formu yalnızca öğrenci oluşturur.

Yerel kurulumda mevcut bir hesabı tek yönetici yapmak için uygulama kapalıyken:

```powershell
dotnet run -- --configure-admin admin
```

Bu komut diğer yönetici rollerini kaldırır. Hesap yoksa rastgele güçlü şifreyle oluşturur ve şifreyi yalnızca komut çıktısında gösterir. Mevcut hesabın şifresini değiştirmez. Kurulum komutu web sunucusunu başlatmadan çıkar.

Ödevler tek öğrenciye atanır. Öğrenci son teslim günü dahil metin ve isteğe bağlı HTTP/HTTPS bağlantısı ile tek teslim yapabilir. Teslimden sonra yönetici 0–100 not ve geri bildirim ekler. Başka öğrenciler görevi okuyamaz veya teslim yapamaz.

## Hesap ve rapor yönetimi

Öğrenciler listesindeki Detay ve yönetim bağlantısından:
- Öğrenci hesabı neden belirtilerek pasife alınabilir veya tekrar aktifleştirilebilir.
- Hesap aktif kalırken tüm mevcut oturumları kapatılabilir.
- Staj bilgileri, ödevler ve günlük raporlar görüntülenebilir.
- Gönderilmiş raporlar geri bildirimle onaylanabilir veya reddedilebilir.

Pasif kullanıcılar SignInManager üzerinden giriş yapamaz. Cookie doğrulaması her istekte aktiflik ve güvenlik damgasını kontrol eder. Oturum sonlandırma bir sonraki sunucu isteğinde uygulanır; yeniden aktifleştirme eski oturumları geri açmaz. Yöneticinin kendi hesabını veya başka yönetici hesabını bu öğrenci işlemleriyle kapatması engellenir. Son giriş zamanı çevrim içi durumunu göstermez.

## Duyurular, takvim ve hesap ayarları

- Yönetici menüsündeki Duyurular sayfasından tüm öğrencilere duyuru yayımlanır. Son gösterim günü isteğe bağlıdır; belirtilen gün dahil duyuru görünür. Arşivlenen veya süresi dolan duyurular öğrencilere gösterilmez.
- Öğrenci Duyurular sayfasında Okudum düğmesi ile kendi okuma bilgisini kaydeder. Panelde okunmamış duyuru sayısı gösterilir.
- Teslim takvimim sayfası öğrencinin kendi ödevlerini aylık gösterir. Önceki/sonraki ay bağlantıları ve bu ayın teslim listesi vardır.
- Üst menüdeki Şifre değiştir sayfasında mevcut şifre doğrulanır. Yeni şifre Identity kurallarına uygun olmalıdır. Başarılı değişiklik mevcut oturumu yeniler; diğer eski oturumlar sonraki isteklerinde geçersiz kalır.
