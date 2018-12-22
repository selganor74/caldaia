@ECHO OFF
SET PATH=%PATH%;C:\Program Files (x86)\MSBuild\14.0\Bin\
SET CONFIGURATION=%1
if "%CONFIGURATION%" == "" (
	echo "Please specifiy a build configuration: %0 Debug|Test|Release|..."
	exit -1
)
pushd %~dp0
	pushd ..
		for %%f in (*.csproj) do (
			echo Building %%f
			msbuild .\%%f /p:Configuration="%CONFIGURATION%" /p:OutputPath="bin\%CONFIGURATION%" /p:Platform="Any CPU" /t:Rebuild 
		)
	popd
popd
