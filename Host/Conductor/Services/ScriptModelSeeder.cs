using Conductor.Data;
using SharedHost.Models.Shell;
using System.Collections.Generic;
using System.Linq;

namespace Conductor.Services
{
    public class ScriptModelSeeder
    {

        public static void SeedScriptModel(ApplicationDbContext dbContext)
        {
            var default_model = new List<ScriptModel>();

            default_model.Add(new ScriptModel()
            {
                ID = 1,
                Name = "GetCpuUsage",
                Script = DefaultScriptModel.GetCpuUsage,
                History = new List<ShellSession>()
            });

            default_model.Add(new ScriptModel()
            {
                ID = 2,
                Name = "GetGPUusage",
                Script = DefaultScriptModel.GetGPUusage,
                History = new List<ShellSession>()
            });


            default_model.Add(new ScriptModel()
            {
                ID = 3,
                Name = "GetRamUsage",
                Script = DefaultScriptModel.GetRamUsage,
                History = new List<ShellSession>()
            });

            default_model.Add(new ScriptModel()
            {
                ID = 4,
                Name = "GetStorageState",
                Script = DefaultScriptModel.GetStorageState,
                History = new List<ShellSession>()
            });


            if (dbContext.ScriptModels.Where(o => o.ID < default_model.Count()).Count() == 0)
            {
                dbContext.ScriptModels.AddRange(default_model);
                dbContext.SaveChanges();
            }
        }
    }
}
