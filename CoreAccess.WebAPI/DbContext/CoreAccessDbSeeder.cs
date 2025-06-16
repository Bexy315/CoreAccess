using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreAccess.WebAPI.Helpers;
using CoreAccess.WebAPI.Model;
using Microsoft.EntityFrameworkCore;

namespace CoreAccess.WebAPI.DbContext;

public static class CoreAccessDbSeeder
{
    public static async Task SeedInitialDataAsync(CoreAccessDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        Console.WriteLine("Starting CoreAccess database seeding...");

        var adminRoleId = Guid.Parse("f63ee75d-f899-4fe5-ae6d-a070164d01fd");
        var userRoleId = Guid.Parse("7bd719cd-fdcc-4463-b633-2aef7208ba38");
        var adminUserId = Guid.Parse("ed82e48e-3686-4ca9-ae06-9a2885e801e0");

        #region Roles

        CoreRole? adminRole = null;

        if (!context.Roles.Any())
        {
            Console.WriteLine("Creating default roles...");

            adminRole = new CoreRole
            {
                Id = adminRoleId,
                Name = "CoreAccess.Admin",
                Description = "CoreAccess Admin role for administrative access to CoreAccess",
                CreatedAt = Now(),
                UpdatedAt = Now(),
                IsSystem = true
            };

            var userRole = new CoreRole
            {
                Id = userRoleId,
                Name = "User",
                Description = "User role for default users",
                CreatedAt = Now(),
                UpdatedAt = Now(),
                IsSystem = false
            };

            context.Roles.AddRange(adminRole, userRole);
            Console.WriteLine("Default roles created.");
        }
        else
        {
            adminRole = await context.Roles.FirstAsync(r => r.Id == adminRoleId);
        }

        #endregion

        #region Permissions

        if (!context.Permissions.Any())
        {
            Console.WriteLine("Creating default permissions...");

            var permissions = new[]
            {
                new CorePermission { Id = Guid.NewGuid(), Name = "user.read", Description = "Read users" },
                new CorePermission { Id = Guid.NewGuid(), Name = "user.write", Description = "Create/update/delete users" },
                new CorePermission { Id = Guid.NewGuid(), Name = "role.read", Description = "Read roles" },
                new CorePermission { Id = Guid.NewGuid(), Name = "role.write", Description = "Create/update/delete roles" },
                new CorePermission { Id = Guid.NewGuid(), Name = "permission.read", Description = "Read permissions" },
                new CorePermission { Id = Guid.NewGuid(), Name = "permission.write", Description = "Manage permissions" },
                new CorePermission { Id = Guid.NewGuid(), Name = "settings.read", Description = "Read system settings" },
                new CorePermission { Id = Guid.NewGuid(), Name = "settings.write", Description = "Update system settings" }
            };

            context.Permissions.AddRange(permissions);
            adminRole!.Permissions.AddRange(permissions);

            Console.WriteLine("Permissions created and assigned to CoreAccess.Admin.");
        }

        #endregion

        #region User

        if (!context.Users.Any())
        {
            Console.WriteLine("Creating default admin user...");

            string username = Environment.GetEnvironmentVariable("COREACCESS_USERNAME") ?? "root";
            string? password = Environment.GetEnvironmentVariable("COREACCESS_PASSWORD");

            if (string.IsNullOrWhiteSpace(password))
            {
                password = SecureKeyHelper.GenerateSecurePassword();
                PrintWarning($"No COREACCESS_PASSWORD provided. Generated password: {password}");
            }

            var user = new CoreUser
            {
                Id = adminUserId,
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                CreatedAt = Now(),
                UpdatedAt = Now(),
                IsSystem = true,
                Status = CoreUserStatus.Active
            };

            user.Roles.Add(adminRole!);
            context.Users.Add(user);

            Console.WriteLine($"Default user created with username: {username}");
        }

        #endregion

        #region AppSettings

        Console.WriteLine("Ensuring required app settings...");

        EnsureSetting(AppSettingsKeys.JwtSecretKey, SecureKeyHelper.GenerateRandomBase64Key(), encrypted: true, system: true);
        EnsureSetting(AppSettingsKeys.JwtIssuer, "coreaccess", encrypted: true, system: true);
        EnsureSetting(AppSettingsKeys.JwtAudience, "coreaccess-client", encrypted: true, system: true);
        EnsureSetting(AppSettingsKeys.JwtExpiresIn, "60", encrypted: true, system: true);
        EnsureSetting(AppSettingsKeys.DisableRegistration, "false", encrypted: false, system: true);
        EnsureSetting(AppSettingsKeys.SystemLogLevel, "Information", encrypted: false, system: true);

        #endregion

        await context.SaveChangesAsync();

        Console.WriteLine("🎉 CoreAccess seeding complete.");
    }

    private static string Now() => DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

    private static void EnsureSetting(string key, string value, bool encrypted, bool system)
    {
    /*    if (!AppSettingsHelper.TryGet(key, out string _, decryptIfNeeded: true))
        {
            Console.WriteLine($"➕ Initializing setting: {key}");
            AppSettingsHelper.Set(key, value, encrypted, system);
        } */
    }

    private static void PrintWarning(string message)
    {
        var previousColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("⚠️  " + message);
        Console.ForegroundColor = previousColor;
    }
}
