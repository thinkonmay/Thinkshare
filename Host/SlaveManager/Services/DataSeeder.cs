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
        private readonly RoleManager<IdentityRole<int>> roleManager;
        private readonly UserManager<UserAccount> userManager;

        public const string ADMIN = "Administrator";
        public const string MOD = "Moderator";
        public const string USER = "User";

        private readonly string[] defaultRoles = { ADMIN, MOD, USER };

        public DataSeeder(
            RoleManager<IdentityRole<int>> _roleManager,
            UserManager<UserAccount> _userManager,
            IConfiguration _config)
        {
            roleManager = _roleManager;
            userManager = _userManager;
        }

        public async Task SeedIdentityAsync()
        {
            await SeedRolesAsync();
            await SeedAdminUsersAsync();
            await SeedUserRoleAsync();
        }

        public async Task SeedRolesAsync()
        {
            foreach (var role in defaultRoles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(role));
                }
            }
        }

        public async Task SeedAdminUsersAsync()
        {
            var admins = await userManager.GetUsersInRoleAsync(ADMIN);
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

                if (await userManager.FindByNameAsync(admin.UserName) == null)
                {
                    var result = await userManager.CreateAsync(admin, defaultPassword);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(admin, ADMIN);
                    }
                }
            }
        }

        public async Task SeedUserRoleAsync()
        {
            foreach (var user in userManager.Users)
            {
                var roles = await userManager.GetRolesAsync(user);
                if (roles.Count == 0) await userManager.AddToRoleAsync(user, USER);
            }
        }
    }
}
