using System.Security.Claims;
using System.Net;
using KeinishkataKushta.Data;
using KeinishkataKushta.Data.Entities;
using KeinishkataKushta.Web.Areas.Admin.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace KeinishkataKushta.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class AccountController : Controller
{
    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    private readonly AppDbContext _db;
    private readonly IPasswordHasher<AdminUser> _passwordHasher;
    private readonly IWebHostEnvironment _environment;

    public AccountController(
        AppDbContext db,
        IPasswordHasher<AdminUser> passwordHasher,
        IWebHostEnvironment environment)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _environment = environment;
    }

    [AllowAnonymous]
    [HttpGet("/Admin/Setup")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Setup()
    {
        if (!CanUseLocalSetup() || await _db.AdminUsers.AnyAsync())
        {
            return NotFound();
        }

        return View(new AdminSetupViewModel());
    }

    [AllowAnonymous]
    [HttpPost("/Admin/Setup")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("admin-login")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Setup(AdminSetupViewModel model)
    {
        if (!CanUseLocalSetup() || await _db.AdminUsers.AnyAsync())
        {
            return NotFound();
        }

        ValidatePasswordStrength(model);
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var username = model.Username.Trim();
        var admin = new AdminUser
        {
            Username = username,
            NormalizedUsername = username.ToUpperInvariant(),
            IsActive = true,
            CreatedUtc = DateTime.UtcNow
        };

        admin.PasswordHash = _passwordHasher.HashPassword(admin, model.Password);
        _db.AdminUsers.Add(admin);
        await _db.SaveChangesAsync();

        TempData["AdminCreated"] = true;
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home", new { area = "Admin" });
        }

        return View(new AdminLoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("admin-login")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Login(AdminLoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var normalizedUsername = model.Username.Trim().ToUpperInvariant();
        var admin = await _db.AdminUsers
            .SingleOrDefaultAsync(user => user.NormalizedUsername == normalizedUsername);

        if (admin == null || !admin.IsActive || admin.LockoutEndUtc > DateTime.UtcNow)
        {
            ModelState.AddModelError(string.Empty, "Невалидно потребителско име или парола.");
            return View(model);
        }

        var verification = _passwordHasher.VerifyHashedPassword(admin, admin.PasswordHash, model.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            admin.AccessFailedCount++;

            if (admin.AccessFailedCount >= MaxFailedAttempts)
            {
                admin.AccessFailedCount = 0;
                admin.LockoutEndUtc = DateTime.UtcNow.Add(LockoutDuration);
            }

            await _db.SaveChangesAsync();
            ModelState.AddModelError(string.Empty, "Невалидно потребителско име или парола.");
            return View(model);
        }

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            admin.PasswordHash = _passwordHasher.HashPassword(admin, model.Password);
        }

        admin.AccessFailedCount = 0;
        admin.LockoutEndUtc = null;
        admin.LastLoginUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
            new Claim(ClaimTypes.Name, admin.Username),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return LocalRedirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Home", new { area = "Admin" });
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private bool CanUseLocalSetup()
    {
        var remoteIp = HttpContext.Connection.RemoteIpAddress;
        return _environment.IsDevelopment() && remoteIp != null && IPAddress.IsLoopback(remoteIp);
    }

    private void ValidatePasswordStrength(AdminSetupViewModel model)
    {
        if (string.IsNullOrEmpty(model.Password))
        {
            return;
        }

        var strongPassword = model.Password.Any(char.IsUpper)
            && model.Password.Any(char.IsLower)
            && model.Password.Any(char.IsDigit)
            && model.Password.Any(character => !char.IsLetterOrDigit(character));

        if (!strongPassword)
        {
            ModelState.AddModelError(nameof(model.Password), "Паролата трябва да съдържа главна, малка буква, цифра и специален знак.");
        }
    }
}
