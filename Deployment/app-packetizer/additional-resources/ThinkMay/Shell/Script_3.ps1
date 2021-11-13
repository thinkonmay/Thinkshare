$Output = Get-Process | Sort-Object WorkingSet64 | Select-Object Name,@{Name='WorkingSet';Expression={($_.WorkingSet64/1MB)}} 
$Final = @()
foreach ( $item in $Output ){
$Usage = $item.WorkingSet 
if($Usage -lt 200){}else{ $Final += $item}}
$Final | ConvertTo-Json