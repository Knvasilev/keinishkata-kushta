using KeinishkataKushta.Data;
using KeinishkataKushta.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace KeinishkataKushta.Infrastructure;

public static class AdminAccountSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        var username = configuration["AdminSeed:Username"]?.Trim();
        var password = configuration["AdminSeed:Password"];

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var strongPassword = password.Length >= 12
            && password.Any(char.IsUpper)
            && password.Any(char.IsLower)
            && password.Any(char.IsDigit)
            && password.Any(character => !char.IsLetterOrDigit(character));

        if (!strongPassword)
        {
            throw new InvalidOperationException("AdminSeed:Password must be at least 12 characters and include upper-case, lower-case, numeric, and special characters.");
        }

        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<AdminUser>>();
        var normalizedUsername = username.ToUpperInvariant();

        if (await db.AdminUsers.AnyAsync(user => user.NormalizedUsername == normalizedUsername))
        {
            return;
        }

        var admin = new AdminUser
        {
            Username = username,
            NormalizedUsername = normalizedUsername,
            IsActive = true,
            CreatedUtc = DateTime.UtcNow
        };

        admin.PasswordHash = passwordHasher.HashPassword(admin, password);
        db.AdminUsers.Add(admin);
        await db.SaveChangesAsync();
    }
}
