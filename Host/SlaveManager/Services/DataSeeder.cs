using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using SlaveManager.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlaveManager.Services
{
    public class DataSeeder
    {
        public const string ADMIN = "Administrator";
        public const string MOD = "Moderator";
        public const string USER = "User";

        public static readonly string[] DEFAULT_ROLES = { ADMIN, MOD, USER };

        public static void SeedRoles(RoleManager<IdentityRole<int>> roleManager)
        {
            foreach (var role in DEFAULT_ROLES)
            {
                if (!roleManager.RoleExistsAsync(role).GetAwaiter().GetResult())
                {
                    roleManager.CreateAsync(new IdentityRole<int>(role)).Wait();
                }
            }
        }

        public static void SeedAdminUsers(UserManager<UserAccount> userManager)
        {
            var admins = userManager.GetUsersInRoleAsync(ADMIN).GetAwaiter().GetResult();
            if (admins.Count == 0)
            {
                UserAccount admin = new UserAccount()
                {
                    UserName = "defadmin",
                    Email = "admin@thinkmay.com",
                    EmailConfirmed = true,
                    FullName = "Default Admin"
                };

                const string defaultPassword = @"@|)|\/|1n|s7ra70r";

                if (userManager.FindByNameAsync(admin.UserName).GetAwaiter().GetResult() == null)
                {
                    var result = userManager.CreateAsync(admin, defaultPassword).GetAwaiter().GetResult();
                    if (result.Succeeded)
                    {
                        userManager.AddToRoleAsync(admin, ADMIN).Wait();
                    }
                }
            }
        }

        public static void SeedUserRole(UserManager<UserAccount> userManager)
        {
            var users = userManager.Users.ToList();
            foreach (var u in users)
            {
                var roles = userManager.GetRolesAsync(u).GetAwaiter().GetResult();
                if (roles.Count == 0) userManager.AddToRoleAsync(u, USER).Wait();
            }
        }
    }
}
