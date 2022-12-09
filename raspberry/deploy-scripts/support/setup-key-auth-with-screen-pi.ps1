# includes variables from config.ps1 file
. .\config.ps1

# generates a new rsa key pair
ssh-keygen.exe -f ${keyPathScreen}

# Waits for the .pub file to be created
Start-Sleep -Second 2
# copies the public key from the generated key pair to the pi, and stores in the authorized keys
Get-Content -Path "${keyPathScreen}.pub" | ssh ${toPiScreen} "mkdir -p ~/.ssh && cat >> ~/.ssh/authorized_keys"

# tests that the key is working
ssh ${toPiScreen} -i ${keyPathScreen} "echo this should work without providing a password!"