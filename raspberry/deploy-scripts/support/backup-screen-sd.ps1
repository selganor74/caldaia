# includes variables from config.ps1 file
. .\config.ps1

Push-Location $backupDir
    ssh -i ${keyPathScreen} ${toPiScreen} 'sudo dd if=/dev/mmcblk0 bs=4096 | gzip -f -c' > .\screen.dd.gz
Pop-Location