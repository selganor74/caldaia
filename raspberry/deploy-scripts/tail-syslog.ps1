# includes variables from config.ps1 file
. .\support\config.ps1

ssh -i ${keyPath} ${toPi} 'tail -f /var/log/syslog -n 100'
