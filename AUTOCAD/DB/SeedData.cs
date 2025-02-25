using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;

namespace AUTOCAD.DB
{
    public class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, RoleManager<IdentityRole> roleManager, ApplicationDbContext dbContext)
        {
            var adminRoleName = "Administrator";
            var userRoleName = "User";

            // Dodawanie roli "Administrator", jeśli nie istnieje
            if (!await roleManager.RoleExistsAsync(adminRoleName))
            {
                await roleManager.CreateAsync(new IdentityRole(adminRoleName));
            }

            // Dodawanie roli "User", jeśli nie istnieje
            if (!await roleManager.RoleExistsAsync(userRoleName))
            {
                await roleManager.CreateAsync(new IdentityRole(userRoleName));
            }

            // Tu możesz dodać dodatkową logikę związaną z dbContext
            // np. inicjalizacja danych w bazie, jeśli jest to wymagane
        }
    }
}
