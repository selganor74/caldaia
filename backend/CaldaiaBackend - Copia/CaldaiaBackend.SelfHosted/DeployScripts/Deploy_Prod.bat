
@ECHO OFF
set CWD=%~dp0

CALL Build.bat Release

pushd %CWD%\..

powershell -ExecutionPolicy Unrestricted DeployScripts\\Deploy.ps1 ^
                    -Username 192.168.2.44\administrator ^
                    -Password caldaia ^
                    -Server 192.168.2.44 ^
                    -NetworkShareName Caldaia ^
                    -DestinationPathInShare \ ^
                    -ServiceName arduinoBackend ^
                    -DeployableServicePath .\\bin\\Release ^
                    -ServiceExecutableFullPathOnServer C:\Caldaia\CaldaiaBackend.SelfHosted.exe ^
                    -ServiceConfigFileName CaldaiaBackend.SelfHosted.exe.config
popd