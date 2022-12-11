# includes variables from config.ps1 file
. .\support\config.ps1

$Env:PATH += ";.\node_modules\.bin"
Push-Location ..\caldaia-frontend\caldaia-frontend\ 
    ng build --base-href /app/
    Push-Location dist\caldaia-frontend 
        scp -i ${keyPath} -r * ${toPi}:caldaia/bin/wwwroot/app
    Pop-Location 
Pop-Location 

Write-Host "Reloading Screen ..."
. .\reload-screen.ps1
Write-Host "... done!"