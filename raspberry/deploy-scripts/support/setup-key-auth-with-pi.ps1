# includes variables from config.ps1 file
. .\config.ps1

# generates a new rsa key pair
ssh-keygen.exe -f ${keyPath}

# copies the public key from the generated key pair to the pi, and stores in the authorized keys
Get-Content ${keyPath}.pub | ssh ${toPi} "mkdir -p ~/.ssh && cat >> ~/.ssh/authorized_keys"

# limits access to current user only (see: https://superuser.com/questions/1309447/how-to-secure-ssh-private-key-on-windows-10 )
.\secure-private-key.ps1 $keyPath

# tests that the key is working
ssh ${toPi} -i ${keyPath} "echo this should work without providing a password!"