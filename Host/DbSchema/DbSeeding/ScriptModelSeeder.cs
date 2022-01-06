using DbSchema.SystemDb.Data;
using SharedHost.Models.Shell;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;

namespace DbSchema.DbSeeding
{
    public class ScriptModelSeeder
    {

        public static void SeedScriptModel(GlobalDbContext dbContext, IHostingEnvironment env)
        {
            var model = new DefaultScriptModel(env);
            var default_model = new List<ScriptModel>();


            default_model.Add(new ScriptModel()
            {
                ID = (int)ScriptModelEnum.GET_CPU,
                Name = "GetCpuUsage",
                Script = model.GetCpuUsage()
            });

            default_model.Add(new ScriptModel()
            {
                ID = (int)ScriptModelEnum.GET_GPU,
                Name = "GetGPUusage",
                Script = model.GetGPUusage()
            });


            default_model.Add(new ScriptModel()
            {
                ID = (int)ScriptModelEnum.GET_RAM,
                Name = "GetRamUsage",
                Script = model.GetRamUsage()
            });

            default_model.Add(new ScriptModel()
            {
                ID = (int)ScriptModelEnum.GET_STORAGE,
                Name = "GetStorageState",
                Script = model.GetStorageState()
            });


            if (dbContext.ScriptModels.Where(o => o.ID < default_model.Count()).Count() == 0)
            {
                dbContext.ScriptModels.AddRange(default_model);
                dbContext.SaveChanges();
            }
        }
    }
}
