using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WriteReview.Domain.Entities;
using WriteReview.Persistence.Contexts;

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

        public static async Task SeedAuthorAsync(UserManager<AppUser> userManager)
        {
            if (userManager == null) throw new ArgumentNullException(nameof(userManager));

            const string authorEmail = "author@writereview.com";
            const string authorPassword = "Author123*";

            var author = await userManager.FindByEmailAsync(authorEmail);

            if (author == null)
            {
                author = new AppUser
                {
                    UserName = authorEmail,
                    Email = authorEmail,
                    FullName = "System Author",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(author, authorPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(author, "Author");
                }
                else
                {
                    throw new Exception($"Author kullanıcı oluşturulamadı: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }

        public static async Task SeedManagerAsync(UserManager<AppUser> userManager)
        {
            if (userManager == null) throw new ArgumentNullException(nameof(userManager));

            const string managerEmail = "manager@writereview.com";
            const string managerPassword = "Manager123*";

            var manager = await userManager.FindByEmailAsync(managerEmail);

            if (manager == null)
            {
                manager = new AppUser
                {
                    UserName = managerEmail,
                    Email = managerEmail,
                    FullName = "System Manager",
                    EmailConfirmed = true,
                };

                var result = await userManager.CreateAsync(manager, managerPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(manager, "Manager");
                }
                else
                {
                    throw new Exception($"Manager kullanıcı oluşturulamadı: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }

        public static async Task SeedExpertAsync(UserManager<AppUser> userManager)
        {
            if (userManager == null) throw new ArgumentNullException(nameof(userManager));

            const string expertEmail = "expert@writereview.com";
            const string expertPassword = "Expert123*";

            var expert = await userManager.FindByEmailAsync(expertEmail);

            if (expert == null)
            {
                expert = new AppUser
                {
                    UserName = expertEmail,
                    Email = expertEmail,
                    FullName = "System Expert",
                    EmailConfirmed = true,
                };

                var result = await userManager.CreateAsync(expert, expertPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(expert, "Expert");
                }
                else
                {
                    throw new Exception($"Expert kullanıcı oluşturulamadı: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }

        public static async Task SeedExpert2Async(UserManager<AppUser> userManager)
        {
            if (userManager == null) throw new ArgumentNullException(nameof(userManager));

            const string expertEmail = "expert2@writereview.com";
            const string expertPassword = "Expert2123*";

            var expert = await userManager.FindByEmailAsync(expertEmail);

            if (expert == null)
            {
                expert = new AppUser
                {
                    UserName = expertEmail,
                    Email = expertEmail,
                    FullName = "System Expert 2",
                    EmailConfirmed = true,
                };

                var result = await userManager.CreateAsync(expert, expertPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(expert, "Expert");
                }
                else
                {
                    throw new Exception($"Expert kullanıcı oluşturulamadı: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }

        public static async Task SeedAdditionalExpertsAsync(UserManager<AppUser> userManager)
        {
            if (userManager == null) throw new ArgumentNullException(nameof(userManager));

            var additionalExperts = new[]
            {
                new { Email = "expert3@writereview.com", Name = "System Expert 3", Password = "Expert3123*" },
                new { Email = "expert4@writereview.com", Name = "System Expert 4", Password = "Expert4123*" },
                new { Email = "expert5@writereview.com", Name = "System Expert 5", Password = "Expert5123*" }
            };

            foreach (var exp in additionalExperts)
            {
                var expert = await userManager.FindByEmailAsync(exp.Email);
                if (expert == null)
                {
                    expert = new AppUser
                    {
                        UserName = exp.Email,
                        Email = exp.Email,
                        FullName = exp.Name,
                        EmailConfirmed = true,
                    };

                    var result = await userManager.CreateAsync(expert, exp.Password);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(expert, "Expert");
                    }
                    else
                    {
                        throw new Exception($"Expert kullanıcı oluşturulamadı: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }
        }

        public static async Task SeedCategoriesAsync(WriteReviewDbContext db)
        {
            var defaultCategories = new[]
            {
                new { Id = Guid.Parse("10000000-0000-0000-0000-000000000001"), Name = "Araştırma Makalesi" },
                new { Id = Guid.Parse("10000000-0000-0000-0000-000000000002"), Name = "Derleme" },
                new { Id = Guid.Parse("10000000-0000-0000-0000-000000000003"), Name = "Olgu Sunumu" },
                new { Id = Guid.Parse("10000000-0000-0000-0000-000000000004"), Name = "Editöre Mektup" },
                new { Id = Guid.Parse("10000000-0000-0000-0000-000000000005"), Name = "Teknik Rapor" },
            };

            foreach (var cat in defaultCategories)
            {
                var exists = await db.Categories.AnyAsync(c => c.Id == cat.Id);
                if (!exists)
                {
                    db.Categories.Add(new Category { Id = cat.Id, Name = cat.Name });
                }
            }

            await db.SaveChangesAsync();
        }

        public static async Task SeedAsync(RoleManager<AppRole> roleManager, UserManager<AppUser> userManager, WriteReviewDbContext db)
        {
            await SeedRolesAsync(roleManager);
            await SeedAdminAsync(userManager);
            await SeedManagerAsync(userManager);
            await SeedExpertAsync(userManager);
            await SeedExpert2Async(userManager);
            await SeedAdditionalExpertsAsync(userManager);
            await SeedAuthorAsync(userManager);
            await SeedCategoriesAsync(db);
        }
    }
}
