SET SCRIPT_DIR=%~dp0
pushd %SCRIPT_DIR%
    net use \\192.168.2.44\D$ /user:192.168.2.44\Administrator caldaia >nul: 2>nul:
    robocopy dist\caldaia-frontend \\192.168.2.44\D$\CaldaiaBackend.SelfHosted_Release\AngularAppDist\caldaia-frontend /S /E /PURGE 
    net use \\192.168.2.44\D$ /D >nul: 2>nul:
popd