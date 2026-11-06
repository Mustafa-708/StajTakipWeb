using Microsoft.AspNetCore.Identity;
using StajTakipWeb.Models;
namespace StajTakipWeb.Data;

public static class AdminSetup
{
    // Yalnızca yerel komut satırından çalışır; web üzerinden rol atama yoktur.
    public static async Task ConfigureAsync(IServiceProvider services, string userName)
    {
        var users = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roles = services.GetRequiredService<RoleManager<IdentityRole>>();
        var db = services.GetRequiredService<StajTakipContext>();
        await using var transaction = await db.Database.BeginTransactionAsync();
        static void Ensure(IdentityResult result)
        {
            if (!result.Succeeded) throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
        }
        if (!await roles.RoleExistsAsync("Yonetici")) Ensure(await roles.CreateAsync(new IdentityRole("Yonetici")));
        var admin = await users.FindByNameAsync(userName);
        string? password = null;
        if (admin == null)
        {
            password = "St!" + Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(12)) + "a9";
            admin = new ApplicationUser { UserName = userName, Email = userName + "@stajtakip.local", Ad = "Sistem", Soyad = "Yöneticisi" };
            Ensure(await users.CreateAsync(admin, password));
        }
        foreach (var previous in await users.GetUsersInRoleAsync("Yonetici"))
        {
            if (previous.Id == admin.Id) continue;
            Ensure(await users.RemoveFromRoleAsync(previous, "Yonetici"));
            Ensure(await users.UpdateSecurityStampAsync(previous));
        }
        if (!await users.IsInRoleAsync(admin, "Yonetici")) Ensure(await users.AddToRoleAsync(admin, "Yonetici"));
        await transaction.CommitAsync();
        Console.WriteLine("Tek yonetici hesabi: " + admin.UserName);
        Console.WriteLine(password == null ? "Mevcut hesabin sifresi korundu." : "Yeni hesap sifresi: " + password);
    }

    public static async Task ResetPasswordAsync(IServiceProvider services, string userName, string newPassword)
    {
        var users = services.GetRequiredService<UserManager<ApplicationUser>>();
        var admin = await users.FindByNameAsync(userName);
        if (admin == null) throw new InvalidOperationException($"'{userName}' kullanıcısı bulunamadı.");
        var token = await users.GeneratePasswordResetTokenAsync(admin);
        var result = await users.ResetPasswordAsync(admin, token, newPassword);
        if (!result.Succeeded) throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
        await users.UpdateSecurityStampAsync(admin);
    }
}
