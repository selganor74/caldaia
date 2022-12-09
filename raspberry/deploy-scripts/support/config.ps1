# This contains variables used by other scripts
$toPi = "pi@192.168.2.44"
$toPiScreen = "pi@192.168.2.43"

$basePath = Resolve-Path $PSScriptRoot 
$keyPath = "$basePath\key.secret"
$keyPathScreen = "$basePath\key-screen.secret"

Write-Host "Using configuration:"
Write-Host "    toPi = $toPi"
Write-Host "    toPiScreen = $toPiScreen"
Write-Host "    keyPath = $keyPath"
Write-Host "    keyPathScreen = $keyPathScreen"