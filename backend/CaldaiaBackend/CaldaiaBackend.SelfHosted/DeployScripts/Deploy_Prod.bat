
@ECHO OFF
set CWD=%~dp0
pushd %CWD%\..

powershell -ExecutionPolicy Unrestricted DeployScripts\\Deploy.ps1 ^
                    -Username 192.168.2.44\admin ^
                    -Password ngaelich ^
                    -Server 192.168.2.44 ^
                    -NetworkShareName caldaiaBackend ^
                    -DestinationPathInShare \ ^
                    -ServiceName arduinoBackend ^
                    -DeployableServicePath .\\bin\\Release ^
                    -ServiceExecutableFullPathOnServer C:\caldaiaBackend\CaldaiaBackend.SelfHosted.exe

popd