# includes variables from config.ps1 file
. .\support\config.ps1

Push-Location ..\caldaia-backend\api\wwwroot
    Remove-Item -Path app -Recurse -Force
Pop-Location

Push-Location ..\caldaia-frontend\caldaia-frontend
    npm run build-prod
Pop-Location

Push-Location ..\caldaia-backend\api
    # generates the final binaries to be published
    dotnet publish --configuration Release
    

    Write-host "Stopping service on pi ..."
    # ssh -i ${keyPath} ${toPi} 'sudo systemctl stop raspberry-caldaia.service'
    ssh -i ${keyPath} ${toPi} './stop'
    Write-host "... done"

    Write-host "Copying files to pi ..."
    Push-Location bin\Release\net6.0\publish\
        scp -i ${keyPath} -r * ${toPi}:caldaia/bin
    Pop-Location

Pop-Location


Write-host "Starting service on pi ..."
ssh -i ${keyPath} ${toPi} 'sudo systemctl start raspberry-caldaia.service'
Write-host "... done"

# . .\deploy-frontend-to-raspberry.ps1

.\tail-debug.ps1