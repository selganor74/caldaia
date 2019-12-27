
@ECHO OFF
set CWD=%~dp0

CALL Build.bat Release

pushd %CWD%\..

powershell -ExecutionPolicy Unrestricted DeployScripts\\Deploy.ps1 ^
                    -Username 192.168.2.44\admin ^
                    -Password caldaia ^
                    -Server 192.168.2.44 ^
                    -NetworkShareName Backend ^
                    -DestinationPathInShare \ ^
                    -ServiceName arduinoBackend ^
                    -DeployableServicePath .\\bin\\Release ^
                    -ServiceExecutableFullPathOnServer C:\caldaiaBackend\CaldaiaBackend.SelfHosted.exe

popd