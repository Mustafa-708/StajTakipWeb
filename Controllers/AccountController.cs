
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajTakipWeb.Data;
using StajTakipWeb.Models;

namespace StajTakipWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly StajTakipContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            StajTakipContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AccountController> logger)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.UserName,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

            
            if (result.Succeeded)
                {
                var signedInUser = await _userManager.FindByNameAsync(model.UserName);
                if (signedInUser != null)
                {
                    signedInUser.SonGiris = DateTime.Now;
                    await _userManager.UpdateAsync(signedInUser);
                }
                return RedirectToAction("Index", "Panel");
                }

            ModelState.AddModelError("",
                result.IsLockedOut
                    ? "Hesabınız geçici olarak kilitlendi."
                    : "Kullanıcı adı veya şifre hatalı.");

            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (await _context.Ogrenciler.AnyAsync(
                x => x.Numara == model.Numara))
            {
                ModelState.AddModelError(
                    "Numara",
                    "Bu öğrenci numarası zaten kayıtlı.");
            }

            if (await _userManager.FindByNameAsync(
                model.UserName) != null)
            {
                ModelState.AddModelError(
                    "UserName",
                    "Bu kullanıcı adı zaten kullanılıyor.");
            }

            if (await _userManager.FindByEmailAsync(
                model.Email) != null)
            {
                ModelState.AddModelError(
                    "Email",
                    "Bu e-posta zaten kullanılıyor.");
            }

            if (!ModelState.IsValid)
                return View(model);

            // Rol mevcut değilse oluştur.
            const string roleName = "Ogrenci";

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var roleResult = await _roleManager.CreateAsync(
                    new IdentityRole(roleName));

                if (!roleResult.Succeeded)
                {
                    ModelState.AddModelError("",
                        "Öğrenci rolü oluşturulamadı.");
                    return View(model);
                }
            }

            // Öğrenci ve kullanıcı aynı işlemde kaydedilir.
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var ogrenci = new Ogrenci
                {
                    Numara = model.Numara,
                    Ad = model.Ad,
                    Soyad = model.Soyad,
                    Bolum = model.Bolum,
                    Sinif = model.Sinif,
                    KayitTarihi = DateTime.Now
                };

                _context.Ogrenciler.Add(ogrenci);
                await _context.SaveChangesAsync();

                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    Ad = model.Ad,
                    Soyad = model.Soyad,
                    OgrenciID = ogrenci.OgrenciID
                };

                var result = await _userManager.CreateAsync(
                    user, model.Password);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("",
                            error.Description);
                    }

                    await transaction.RollbackAsync();
                    return View(model);
                }

                var roleResult = await _userManager
                    .AddToRoleAsync(user, roleName);

                if (!roleResult.Succeeded)
                {
                    await transaction.RollbackAsync();

                    ModelState.AddModelError("",
                        "Kullanıcı rolü atanamadı.");

                    return View(model);
                }

                await transaction.CommitAsync();

                TempData["SuccessMessage"] =
                    "Kayıt başarılı! Giriş yapabilirsiniz.";

                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Öğrenci kaydı veritabanına kaydedilemedi.");

                ModelState.AddModelError("",
                    "Kayıt sırasında bir hata oluştu.");

                return View(model);
            }
        }

        [Authorize]
        [HttpGet]
        public IActionResult SifreDegistir() => View(new SifreDegistirForm());

        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SifreDegistir(SifreDegistirForm form)
        {
            if (!ModelState.IsValid) return View(form);
            if (form.MevcutSifre == form.YeniSifre) { ModelState.AddModelError("YeniSifre", "Yeni şifre mevcut şifreden farklı olmalıdır."); return View(form); }
            var user = await _userManager.GetUserAsync(User);
            if (user == null || !user.AktifMi) return Forbid();
            var result = await _userManager.ChangePasswordAsync(user, form.MevcutSifre, form.YeniSifre);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors) ModelState.AddModelError("", error.Code == "PasswordMismatch" ? "Mevcut şifreniz yanlış." : "Yeni şifreniz en az 8 karakter, büyük harf, küçük harf ve rakam içermelidir.");
                return View(form);
            }
            await _signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "Şifreniz değiştirildi. Diğer cihazlardaki eski oturumlar bir sonraki istekte kapatılacak.";
            return RedirectToAction(nameof(SifreDegistir));
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return Content(
                "Bu sayfaya erişim yetkiniz yok.");
        }
    }
}
