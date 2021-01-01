
@ECHO OFF
SETLOCAL EnableDelayedExpansion

SET CurrentDir=%~dp0
SET CONFIGURATION=%1
SET PATH=%PATH%;C:\Program Files (x86)\MSBuild\14.0\Bin\;%CurrentDir%


pushd %CurrentDir%\..
	SET ProjectDir=!CD!
popd

pushd %CurrentDir%\..\..
	SET SolutionDir=!CD!
popd

ECHO ProjectDir: %ProjectDir%
ECHO SolutionDir: %SolutionDir%

pushd %SolutionDir%
	ECHO ***************************************************************************************
	ECHO  BUILDING all .sln in %CD%
	ECHO ***************************************************************************************
		for %%p in (*.sln) do (
			echo Building %%p
			msbuild .\%%p /p:Configuration="%CONFIGURATION%" /p:OutputPath="bin\%CONFIGURATION%" /p:Platform="Any CPU" /t:Rebuild  /p:ProjectDir="%ProjectDir%" /p:WarningLevel=1 /p:SolutionDir="%SolutionDir%" -verbosity:minimal
			IF ERRORLEVEL 1 GOTO :BuildError
		)
	)
popd

EXIT /B 0

:BuildError
REM Errors occoured in build process
EXIT /B 1