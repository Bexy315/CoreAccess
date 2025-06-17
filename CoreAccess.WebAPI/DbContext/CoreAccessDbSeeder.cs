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

        #region Roles

        CoreRole? adminRole = null;

        if (!context.Roles.Any())
        {
            Console.WriteLine("Creating default roles...");

            adminRole = new CoreRole
            {
                Id = Guid.Parse("f63ee75d-f899-4fe5-ae6d-a070164d01fd"),
                Name = "CoreAccess.Admin",
                Description = "CoreAccess Admin role for administrative access to CoreAccess",
                CreatedAt = Now(),
                UpdatedAt = Now(),
                IsSystem = true
            };

            var userRole = new CoreRole
            {
                Id = Guid.Parse("7bd719cd-fdcc-4463-b633-2aef7208ba38"),
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
            adminRole = await context.Roles.FirstAsync(r => r.Id == Guid.Parse("f63ee75d-f899-4fe5-ae6d-a070164d01fd"));
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

        await context.SaveChangesAsync();

        Console.WriteLine("🎉 CoreAccess seeding complete.");
    }

    private static string Now() => DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
}
