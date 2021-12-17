$GpuMemTotal = (((Get-Counter "\GPU Process Memory(*)\Local Usage").CounterSamples | where CookedValue).CookedValue | measure -sum).sum 
$GpuUseTotal = (((Get-Counter "\GPU Engine(*engtype_3D)\Utilization Percentage").CounterSamples | where CookedValue).CookedValue | measure -sum).sum 
@"
{
"GPUMem": $([math]::Round($GpuMemTotal/1MB,2)),
"GPUEngine": $([math]::Round($GpuUseTotal,2))
}
"@