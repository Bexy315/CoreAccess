using System;
using System.Linq;
using System.Threading.Tasks;
using CoreAccess.WebAPI.Model;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using CoreAccess.WebAPI.Helpers;

namespace CoreAccess.WebAPI.DbContext;

public static class CoreAccessDbSeeder
{
    public static async Task SeedInitialDataAsync(CoreAccessDbContext context)
    {
        // Stelle sicher, dass die DB existiert
        await context.Database.EnsureCreatedAsync();

        // Admin-Rolle abrufen (wurde bereits via HasData angelegt)
        var adminRoleId = Guid.Parse("f63ee75d-f899-4fe5-ae6d-a070164d01fd");
        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Id == adminRoleId);

        if (adminRole == null)
        {
            throw new Exception("Admin role not found – ensure EF seeding is executed first.");
        }

        if (context.Users.Any()) return;

        string username = Environment.GetEnvironmentVariable("COREACCESS_USERNAME") ?? "root";

        string? password = Environment.GetEnvironmentVariable("COREACCESS_PASSWORD");

        if (string.IsNullOrWhiteSpace(password))
        {
            password = SecureKeyHelper.GenerateSecurePassword();
            Console.WriteLine("!!------------------------------------------------------------------!!");
            Console.WriteLine("!! No COREACCESS_PASSWORD was provided. A random password was generated: {0} !!", password);
            Console.WriteLine("!!------------------------------------------------------------------!!");
        }

        string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new CoreUser
        {
            Id = Guid.NewGuid(),
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            CreatedAt = DateTime.UtcNow.ToString("o"),
            UpdatedAt = DateTime.UtcNow.ToString("o"),
            Roles = [adminRole],
            IsSystem = true,
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();
    }
}