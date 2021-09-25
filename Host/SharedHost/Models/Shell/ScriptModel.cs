using SharedHost.Models.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SharedHost.Models.Shell
{
    public class ScriptModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }

        public string Name { get; set; }

        public string Script { get; set; }

        public virtual ICollection<ShellSession> History { get; set; }
    }


    public class DefaultScriptModel
    {
        public string GetCpuUsage = 
            "$CpuCores = (Get-WMIObject Win32_ComputerSystem).NumberOfLogicalProcessors\n" +
            "$Samples = (Get-Counter \"\\Process($Processname*)\\% Processor Time\").CounterSamples\n" +
            "$Samples | Select `\n" +
            "InstanceName,\n" +
            "@{Name=\"CPUpercentage\";Expression={[Decimal]::Round(($_.CookedValue / $CpuCores), 2)}} | ConvertTo-Json";

        public string GetRamUsage = 
            "Get-Process | Sort-Object WorkingSet64 | Select-Object Name,@{Name='WorkingSet';Expression={($_.WorkingSet64/1MB)}} | ConvertTo-Json";

        public string GetStorageState = 
            "(get-wmiobject -class win32_logicaldisk) | ConvertTo-Json";

        public string GetGPUusage = 
            "$GpuMemTotal = (((Get-Counter \"\\GPU Process Memory(*)\\Local Usage\").CounterSamples | where CookedValue).CookedValue | measure -sum).sum \n" +
            "$GpuUseTotal = (((Get-Counter \"\\GPU Engine(*engtype_3D)\\Utilization Percentage\").CounterSamples | where CookedValue).CookedValue | measure -sum).sum \n" +
            "@\"\n" +
            "{\n" +
            "\"GPUMem\": $([math]::Round($GpuMemTotal/1MB,2)),\n" +
            "\"GPUEngine\": $([math]::Round($GpuUseTotal,2)),\n" +
            "}\n" +
            "\"@";
    }
}