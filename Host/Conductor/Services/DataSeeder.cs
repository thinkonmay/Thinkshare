using Microsoft.AspNetCore.Identity;
using SharedHost.Models.User;
using System.Linq;
using SharedHost;
using Conductor.Data;
using SharedHost.Models.Shell;
using System.Collections.Generic;

namespace Conductor.Services
{
    public class DataSeeder
    {
        public const string ADMIN = "Administrator";
        public const string MOD = "Moderator";
        public const string USER = "User";


        public const string GetCpuUsage =
        "$CpuCores = (Get-WMIObject Win32_ComputerSystem).NumberOfLogicalProcessors\n" +
        "$Samples = (Get-Counter \"\\Process($Processname*)\\% Processor Time\").CounterSamples\n" +
        "$Samples | Select `\n" +
        "InstanceName,\n" +
        "@{Name=\"CPUpercentage\";Expression={[Decimal]::Round(($_.CookedValue / $CpuCores), 2)}} | ConvertTo-Json";

        public const string GetRamUsage =
        "Get-Process | Sort-Object WorkingSet64 | Select-Object Name,@{Name='WorkingSet';Expression={($_.WorkingSet64/1MB)}} | ConvertTo-Json";

        public const string GetStorageState =
         "(get-wmiobject -class win32_logicaldisk) | ConvertTo-Json";

        public const string GetGPUusage =
        "$GpuMemTotal = (((Get-Counter \"\\GPU Process Memory(*)\\Local Usage\").CounterSamples | where CookedValue).CookedValue | measure -sum).sum \n" +
        "$GpuUseTotal = (((Get-Counter \"\\GPU Engine(*engtype_3D)\\Utilization Percentage\").CounterSamples | where CookedValue).CookedValue | measure -sum).sum \n" +
        "@\"\n" +
        "{\n" +
        "\"GPUMem\": $([math]::Round($GpuMemTotal/1MB,2)),\n" +
        "\"GPUEngine\": $([math]::Round($GpuUseTotal,2))\n" +
        "}\n" +
        "\"@";

        

        public static readonly string[] DEFAULT_ROLES = { ADMIN, MOD, USER };

        public static readonly string[] DEFAULT_SCRIPT = { GetCpuUsage, GetRamUsage, GetStorageState, GetGPUusage };

        public static void SeedScriptModel(ApplicationDbContext dbContext)
        {
            var default_model = new List<ScriptModel>();

            default_model.Add(new ScriptModel()
            {
                ID = 1,
                Name = "GetCpuUsage",
                Script = GetCpuUsage,
                History = new List<ShellSession>()
            });

            default_model.Add(new ScriptModel()
            {
                ID = 2,
                Name = "GetGPUusage",
                Script = GetGPUusage,
                History = new List<ShellSession>()
            });


            default_model.Add(new ScriptModel()
            {
                ID = 3,
                Name = "GetRamUsage",
                Script = GetRamUsage,
                History = new List<ShellSession>()
            });

            default_model.Add(new ScriptModel()
            {
                ID = 4,
                Name = "GetStorageState",
                Script = GetStorageState,
                History = new List<ShellSession>()
            });


            if (dbContext.ScriptModels.Where(o => o.ID < default_model.Count()).Count() == 0)
            {
                dbContext.ScriptModels.AddRange(default_model);
                dbContext.SaveChanges();
            }
        }

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



        public static void SeedAdminUsers(UserManager<UserAccount> userManager,SystemConfig config)
        {
            var admins = userManager.GetUsersInRoleAsync(ADMIN).GetAwaiter().GetResult();
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
