$CpuCores = (Get-WMIObject Win32_ComputerSystem).NumberOfLogicalProcessors
$Samples = (Get-Counter "\Process($Processname*)\% Processor Time").CounterSamples
$Samples | Select `
InstanceName,
@{Name="CPUpercentage";Expression={[Decimal]::Round(($_.CookedValue / $CpuCores), 2)}} | ConvertTo-Json