$storage = (get-wmiobject -class win32_logicaldisk)
$ret = @()
foreach ( $node in $storage )
{
    $temp = @{
    "DeviceID"= $node.DeviceID
    "DeviceType"= $node.DeviceType
    "FreeSpace"= $([math]::Round($node.FreeSpace/1GB))
    "Size"= $([math]::Round($node.Size/1GB))
    }
    $ret += $temp
}
$ret | ConvertTo-Json