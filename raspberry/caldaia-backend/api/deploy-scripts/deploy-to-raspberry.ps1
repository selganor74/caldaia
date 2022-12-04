# includes variables from config.ps1 file
. .\support\config.ps1

Push-Location ..
    # generates the final binaries to be published
    dotnet publish

    Write-host "Stopping service on pi ..."
    # ssh -i ${keyPath} ${toPi} 'sudo systemctl stop raspberry-caldaia.service'
    ssh -i ${keyPath} ${toPi} './stop'
    Write-host "... done"

    Write-host "Copying files to pi ..."
    scp -i ${keyPath} -r bin\Debug\net6.0\publish\* ${toPi}:caldaia/bin

    Write-host "Starting service on pi ..."
    ssh -i ${keyPath} ${toPi} 'sudo systemctl start raspberry-caldaia.service'
    Write-host "... done"

Pop-Location

.\tail-debug.ps1