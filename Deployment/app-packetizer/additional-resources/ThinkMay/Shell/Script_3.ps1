$CpuCores = (Get-WMIObject Win32_ComputerSystem).NumberOfLogicalProcessors
$Samples = (Get-Counter "\Process($Processname*)\% Processor Time").CounterSamples
$Output = $Samples | Select `
InstanceName,
@{Name="CPUpercentage";Expression={[Decimal]::Round(($_.CookedValue / $CpuCores), 2)}} 
$Final = @()
foreach ( $item in $Output ){
$Usage = $item.CPUpercentage 
if($Usage -lt 1){}else{ $Final += $item}}
$Final | ConvertTo-Json