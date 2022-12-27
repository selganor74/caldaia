# includes variables from config.ps1 file
. .\config.ps1

Push-Location $backupDir
    ssh -i ${keyPath} ${toPi} 'sudo dd if=/dev/mmcblk0 bs=4096 | gzip -f -c' > .\backend.dd.gzip
Pop-Location