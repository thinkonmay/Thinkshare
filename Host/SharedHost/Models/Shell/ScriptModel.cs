using SharedHost.Models.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Hosting;

namespace SharedHost.Models.Shell
{
    public class ScriptModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }

        public string Name { get; set; }

        public string Script { get; set; }
    }

    public class DefaultScriptModel
    {
        private IHostingEnvironment _env;
        public DefaultScriptModel(IHostingEnvironment env)
        {
            _env = env;
        }
        public string GetCpuUsage() 
        {
            return System.IO.File.ReadAllText(System.IO.Path.Combine(_env.WebRootPath,@".\GetCPUUsage.ps1"));
        }
    
        public string GetRamUsage ()
        {
            return System.IO.File.ReadAllText(System.IO.Path.Combine(_env.WebRootPath,@".\GetRAMUsage.ps1"));
        }

        public string GetStorageState () 
        {
            return System.IO.File.ReadAllText(System.IO.Path.Combine(_env.WebRootPath,@".\GetStorage.ps1"));
        }

        public string GetGPUusage ()
        {
            return System.IO.File.ReadAllText(System.IO.Path.Combine(_env.WebRootPath,@".\GetGPUUsage.ps1"));
        } 

    }
    public enum ScriptModelEnum
    {
        GET_CPU = 1,
        GET_GPU,
        GET_RAM,
        GET_STORAGE,

        LAST_DEFAULT_MODEL
    }
}