
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using StajTakipWeb.Data;
using StajTakipWeb.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// SQL Server bağlantısı
var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<StajTakipContext>(
    options => options.UseSqlServer(connectionString)
);

// Identity sistemi
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;

        options.User.RequireUniqueEmail = true;
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
    })
    .AddEntityFrameworkStores<StajTakipContext>()
    .AddSignInManager<ActiveAccountSignInManager>()
    .AddDefaultTokenProviders();

builder.Services.Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = TimeSpan.Zero);

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    options.Events.OnValidatePrincipal = async context =>
    {
        await SecurityStampValidator.ValidatePrincipalAsync(context);
        if (context.Principal?.Identity?.IsAuthenticated != true) return;
        var manager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await manager.GetUserAsync(context.Principal);
        if (user == null || !user.AktifMi)
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SingleAdmin", policy => policy.RequireRole("Yonetici").RequireAssertion(async context =>
    {
        if (context.Resource is not HttpContext http) return false;
        var manager = http.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        var admins = await manager.GetUsersInRoleAsync("Yonetici");
        return admins.Count == 1 && admins[0].Id == manager.GetUserId(context.User);
    }));
});

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    await OgrenciPanelSchema.EnsureAsync(scope.ServiceProvider.GetRequiredService<StajTakipContext>());
}

if (builder.Configuration["configure-admin"] is string adminName)
{
    await using var adminScope = app.Services.CreateAsyncScope();
    var resetPassword = Environment.GetEnvironmentVariable("STAJ_ADMIN_PASSWORD");
    if (!string.IsNullOrWhiteSpace(resetPassword))
    {
        await AdminSetup.ResetPasswordAsync(adminScope.ServiceProvider, adminName, resetPassword);
        return;
    }
    await AdminSetup.ConfigureAsync(adminScope.ServiceProvider, adminName);
    return;
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Yerel HTTP geliştirme ortamı
// app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Login sayfası ana sayfamız
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
