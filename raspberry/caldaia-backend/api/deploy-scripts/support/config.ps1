# This contains variables used by other scripts
$toPi = "pi@192.168.2.41"
$keyPath = Resolve-Path $PSScriptRoot\key.secret

Write-Host "Using configuration:"
Write-Host "    toPi = $toPi"
Write-Host "    keyPath = $keyPath"