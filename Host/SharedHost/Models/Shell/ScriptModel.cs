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
    }

    public class DefaultScriptModel
    {
        public const string GetCpuUsage =
        "$CpuCores = (Get-WMIObject Win32_ComputerSystem).NumberOfLogicalProcessors\n" +
        "$Samples = (Get-Counter \"\\Process($Processname*)\\% Processor Time\").CounterSamples\n" +
        "$Samples | Select `\n" +
        "InstanceName,\n" +
        "@{Name=\"CPUpercentage\";Expression={[Decimal]::Round(($_.CookedValue / $CpuCores), 2)}}\n"+
        "$Final = @()\n"+
        "foreach ( $item in $Output ){\n"+
        "$Usage = $item.CPUpercentage \n"+
        "if($Usage -lt 1){}else{ $Final += $item}}\n"+
        "$Final | ConvertTo-Json\n";
        
        public const string GetRamUsage =
        "Get-Process | Sort-Object WorkingSet64 | Select-Object Name,@{Name='WorkingSet';Expression={($_.WorkingSet64/1MB)}}\n" +
        "$Final = @()\n" +
        "foreach ( $item in $Output ){\n" +
        "$Usage = $item.WorkingSet \n" +
        "if($Usage -lt 200){}else{ $Final += $item}}\n" +
        "$Final | ConvertTo-Json\n";

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