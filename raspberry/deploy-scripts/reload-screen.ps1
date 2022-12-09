# includes variables from config.ps1 file
. .\support\config.ps1

ssh -i ${keyPathScreen} ${toPiScreen} './reload-chromium'
