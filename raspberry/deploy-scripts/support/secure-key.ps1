# This script makes sure that the private key used to access a passwordless ssh are secured
# in terms of users that are able to access the key 
. .\config.ps1

.\secure-private-key.ps1 $keyPath
.\secure-private-key.ps1 $keyPathScreen

# tests that the key is working
ssh ${toPi} -i ${keyPath} "echo this should work without providing a password!"

# tests that the key is working
ssh ${toPiScreen} -i ${keyPathScreen} "echo this should work without providing a password!"