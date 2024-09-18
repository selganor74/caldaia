# Sets permission on the provided file (usually an rsa private key)
# ssh will complain and not use the key, asking for a password each time ...

param(
    [Parameter(Mandatory=$true)]
    [string]$KeyPath
)

# Remove Inheritance:
Icacls $KeyPath /c /t /Inheritance:d

# Set Ownership to Owner:
Icacls $KeyPath /c /t /Grant ${env:UserName}:F

# Remove All Users, except for Owner:
Icacls $KeyPath  /c /t /Remove Administrator BUILTIN\Administrators BUILTIN Everyone System Users "NT AUTHORITY\Authenticated Users"

# Verify:
Icacls $KeyPath