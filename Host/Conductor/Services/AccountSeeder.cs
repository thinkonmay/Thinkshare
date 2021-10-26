using Microsoft.AspNetCore.Identity;
using SharedHost.Models.User;
using System.Linq;
using SharedHost;
using DbSchema.SystemDb.Data;
using SharedHost.Models.Shell;
using System.Collections.Generic;
using DbSchema.DbSeeding;

namespace Conductor.Services
{
    public class AccountSeeder
    {
        public static void SeedRoles(RoleManager<IdentityRole<int>> roleManager)
        {
            foreach (var role in RoleSeeding.DEFAULT_ROLES)
            {
                if (!roleManager.RoleExistsAsync(role).GetAwaiter().GetResult())
                {
                    roleManager.CreateAsync(new IdentityRole<int>(role)).Wait();
                }
            }
        }



        public static void SeedAdminUsers(UserManager<UserAccount> userManager,SystemConfig config)
        {
            var admins = userManager.GetUsersInRoleAsync(RoleSeeding.ADMIN).GetAwaiter().GetResult();
            if (admins.Count == 0)
            {
                UserAccount admin = new UserAccount()
                {
                    UserName = config.AdminLogin.UserName,
                    Email = config.AdminLogin.UserName,
                    FullName = "Default Admin",
                    EmailConfirmed = true,
                };

                string defaultPassword = config.AdminLogin.Password;

                if (userManager.FindByNameAsync(admin.UserName).GetAwaiter().GetResult() == null)
                {
                    var result = userManager.CreateAsync(admin, defaultPassword).GetAwaiter().GetResult();
                    if (result.Succeeded)
                    {
                        userManager.AddToRoleAsync(admin, RoleSeeding.ADMIN).Wait();
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
                if (roles.Count == 0) userManager.AddToRoleAsync(u, RoleSeeding.USER).Wait();
            }
        }
    }
}
