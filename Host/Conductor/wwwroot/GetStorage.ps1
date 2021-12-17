$storage = (get-wmiobject -class win32_logicaldisk)
$storage | ConvertTo-Json