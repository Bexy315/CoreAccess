using System;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
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
        await context.Database.EnsureCreatedAsync();

        #region Roles
        
        // Rollen initialisieren
        CoreRole[] roles;
        
        if (!context.Roles.Any())
        {
            Console.WriteLine("Initialising default roles...");
            
            roles = new[]
            {
                new CoreRole
                {
                    Id = Guid.Parse("f63ee75d-f899-4fe5-ae6d-a070164d01fd"),
                    Name = "CoreAccess.Admin",
                    Description = "CoreAccess Admin role for administrative access to CoreAccess",
                    CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    IsSystem = true
                },
                new CoreRole
                {
                    Id = Guid.Parse("7bd719cd-fdcc-4463-b633-2aef7208ba38"),
                    Name = "User",
                    Description = "User role for Default Users",
                    CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    IsSystem = false
                }
            };

            context.Roles.AddRange(roles);
            
            Console.WriteLine("Default roles successfully initialized.");
        }else
        {
            roles = context.Roles.ToArray();
        }
        
        #endregion

        #region User

        // User initialisieren
        if (!context.Users.Any())
        {

            Console.WriteLine("Initialising default user...");

            string username = Environment.GetEnvironmentVariable("COREACCESS_USERNAME") ?? "root";

            string? password = Environment.GetEnvironmentVariable("COREACCESS_PASSWORD");

            if (string.IsNullOrWhiteSpace(password))
            {
                password = SecureKeyHelper.GenerateSecurePassword();
                Console.WriteLine(
                    "!!-------------------------------------------------------------------------------------------------------------------!!");
                Console.WriteLine("!! No COREACCESS_PASSWORD was provided. A random password was generated: {0} !!",
                    password);
                Console.WriteLine(
                    "!!-------------------------------------------------------------------------------------------------------------------!!");
            }

            var user = new CoreUser
            {
                Id = Guid.Parse("ed82e48e-3686-4ca9-ae06-9a2885e801e0"),
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                IsSystem = true,
                Status = CoreUserStatus.Active,
            };
            
            user.Roles.Add(roles.FirstOrDefault(r => r.Id == Guid.Parse("f63ee75d-f899-4fe5-ae6d-a070164d01fd")));

            context.Users.Add(user);

            Console.WriteLine("Default user successfully initialized with username: {0}", username);
        }

        #endregion
        
        #region AppSettings
        
        // AppSettings initialisieren
        
        if (!AppSettingsHelper.TryGet(AppSettingsKeys.JwtSecretKey, out string? secret, decryptIfNeeded: true))
        {
            Console.WriteLine("Initialisiere JwtSecretKey...");
            AppSettingsHelper.Set(AppSettingsKeys.JwtSecretKey, SecureKeyHelper.GenerateRandomBase64Key(), true, true);
        }

        if (!AppSettingsHelper.TryGet(AppSettingsKeys.JwtIssuer, out string? issuer, decryptIfNeeded: true))
        {
            Console.WriteLine("Initialisiere JwtIssuer...");
            AppSettingsHelper.Set(AppSettingsKeys.JwtIssuer, "coreaccess", true, true);
        }

        if (!AppSettingsHelper.TryGet(AppSettingsKeys.JwtAudience, out string? audience, decryptIfNeeded: true))
        {
            Console.WriteLine("Initialisiere JwtAudience...");
            AppSettingsHelper.Set(AppSettingsKeys.JwtAudience, "coreaccess-client", true, true);
        }

        if (!AppSettingsHelper.TryGet(AppSettingsKeys.JwtExpiresIn, out string? expiresIn, decryptIfNeeded: true))
        {
            Console.WriteLine("Initialisiere JwtExpiresIn...");
            AppSettingsHelper.Set(AppSettingsKeys.JwtExpiresIn, "60", true, true);
        }

        if (!AppSettingsHelper.TryGet(AppSettingsKeys.DisableRegistration, out string? disableRegistration))
        {
            Console.WriteLine("Initialisiere DisableRegistration...");
            AppSettingsHelper.Set(AppSettingsKeys.DisableRegistration, "false", false, true);
        }
        
        if (!AppSettingsHelper.TryGet(AppSettingsKeys.SystemLogLevel, out string? systemLogLevel))
        {
            Console.WriteLine("Initialisiere SystemLogLevel...");
            AppSettingsHelper.Set(AppSettingsKeys.SystemLogLevel, "Information", false, true);
        }
        
        #endregion
        
        await context.SaveChangesAsync();
    }
}