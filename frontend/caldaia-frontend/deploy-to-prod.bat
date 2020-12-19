SET SCRIPT_DIR=%~dp0
pushd %SCRIPT_DIR%
    net use \\192.168.2.44\Caldaia /user:192.168.2.44\caldaia caldaia >nul: 2>nul:
    robocopy dist\caldaia-frontend \\192.168.2.44\Caldaia\AngularAppDist\caldaia-frontend /S /E /PURGE 
    net use \\192.168.2.44\Caldaia /D >nul: 2>nul:
popd