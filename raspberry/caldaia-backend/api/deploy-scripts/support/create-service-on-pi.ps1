# includes variables from config.ps1 file
. .\config.ps1

# Copies the reaspberry-caldaia.service to the users home directory ...
Get-Content raspberry-caldaia.service | ssh ${toPi} -i ${keyPath} 'cat >> raspberry-caldaia.service' 
# ... then moves the file to the correct location, and reloads the systemd configuration ...
ssh ${toPi} -i ${keyPath} 'sudo mv raspberry-caldaia.service /lib/systemd/system/ && sudo systemctl daemon-reload'
# ... finally enables the service so it will start at system boot
ssh ${toPi} -i ${keyPath} 'sudo systemctl enable raspberry-caldaia.service'

Write-Output "Ok"
