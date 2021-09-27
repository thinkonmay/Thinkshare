using Conductor.Data;
using SharedHost.Models.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Conductor.Services
{
    public class ScriptModelSeeder
    {
        public const string GetCpuUsage =
        "$CpuCores = (Get-WMIObject Win32_ComputerSystem).NumberOfLogicalProcessors\n" +
        "$Samples = (Get-Counter \"\\Process($Processname*)\\% Processor Time\").CounterSamples\n" +
        "$Samples | Select `\n" +
        "InstanceName,\n" +
        "@{Name=\"CPUpercentage\";Expression={[Decimal]::Round(($_.CookedValue / $CpuCores), 2)}} | ConvertTo-Json";

        public const string GetRamUsage =
        "Get-Process | Sort-Object WorkingSet64 | Select-Object Name,@{Name='WorkingSet';Expression={($_.WorkingSet64/1MB)}} | ConvertTo-Json";

        public const string GetStorageState =
        "$storage = (get-wmiobject -class win32_logicaldisk)\n" +
        "$ret = @()\n" +
        "foreach ( $node in $storage )\n" +
        "{\n" +
        "    $temp = @{\n" +
        "    \"DeviceID\"= $node.DeviceID\n" +
        "    \"DeviceType\"= $node.DeviceType\n" +
        "    \"FreeSpace\"= $([math]::Round($node.FreeSpace/1GB))\n" +
        "    \"Size\"= $([math]::Round($node.Size/1GB))\n" +
        "    }\n" +
        "    $ret += $temp\n" +
        "}\n" +
        "$ret | ConvertTo-Json";

        public const string GetGPUusage =
        "$GpuMemTotal = (((Get-Counter \"\\GPU Process Memory(*)\\Local Usage\").CounterSamples | where CookedValue).CookedValue | measure -sum).sum \n" +
        "$GpuUseTotal = (((Get-Counter \"\\GPU Engine(*engtype_3D)\\Utilization Percentage\").CounterSamples | where CookedValue).CookedValue | measure -sum).sum \n" +
        "@\"\n" +
        "{\n" +
        "\"GPUMem\": $([math]::Round($GpuMemTotal/1MB,2)),\n" +
        "\"GPUEngine\": $([math]::Round($GpuUseTotal,2))\n" +
        "}\n" +
        "\"@";

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
    }
}
