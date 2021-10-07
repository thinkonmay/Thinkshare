﻿using Conductor.Data;
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
                ID = (int)ScriptModelEnum.GET_CPU,
                Name = "GetCpuUsage",
                Script = DefaultScriptModel.GetCpuUsage
            });

            default_model.Add(new ScriptModel()
            {
                ID = (int)ScriptModelEnum.GET_GPU,
                Name = "GetGPUusage",
                Script = DefaultScriptModel.GetGPUusage
            });


            default_model.Add(new ScriptModel()
            {
                ID = (int)ScriptModelEnum.GET_RAM,
                Name = "GetRamUsage",
                Script = DefaultScriptModel.GetRamUsage
            });

            default_model.Add(new ScriptModel()
            {
                ID = (int)ScriptModelEnum.GET_STORAGE,
                Name = "GetStorageState",
                Script = DefaultScriptModel.GetStorageState
            });


            if (dbContext.ScriptModels.Where(o => o.ID < default_model.Count()).Count() == 0)
            {
                dbContext.ScriptModels.AddRange(default_model);
                dbContext.SaveChanges();
            }
        }
    }
}