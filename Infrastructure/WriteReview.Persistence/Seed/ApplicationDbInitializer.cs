using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WriteReview.Domain.Entities;

namespace WriteReview.Persistence.Seed
{
    public class ApplicationDbInitializer
    {
        public static async Task SeedRolesAsync(RoleManager<AppRole> roleManager)
        {
            if(roleManager == null)
                throw new ArgumentNullException(nameof(roleManager));

            string[] roleNames = { "Admin", "Author", "Expert", "Manager" };

            foreach (var role in roleNames)
            {
                var exists = await roleManager.RoleExistsAsync(role);
                if (!exists)
                {
                    await roleManager.CreateAsync(new AppRole { Name = role});
                }
            }
        }

        public static async Task SeedAdminAsync(UserManager<AppUser> userManager)
        {
            if (userManager == null) throw new ArgumentNullException(nameof(userManager));

            const string adminEmail = "admin@writereview.com";
            const string adminPassword = "Admin123*";

            var admin = await userManager.FindByEmailAsync(adminEmail);

            if (admin == null)
            {
                admin = new AppUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin"); 
                }
                else
                {
                    throw new Exception($"Admin kullanıcı oluşturulamadı: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }

        public static async Task SeedAsync(RoleManager<AppRole> roleManager, UserManager<AppUser> userManager)
        {
            await SeedRolesAsync(roleManager);
            await SeedAdminAsync(userManager);
        }
    }
}
