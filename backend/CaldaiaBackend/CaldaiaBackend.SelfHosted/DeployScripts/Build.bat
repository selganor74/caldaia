@ECHO OFF
SET PATH=%PATH%;C:\Program Files (x86)\MSBuild\14.0\Bin\
REM SET CONFIGURATION=%1
REM if "%CONFIGURATION%" == "" (
REM	  echo "Please specifiy a build configuration: %0 Debug|Test|Release|..."
REM	  exit -1
REM )
pushd %~dp0
	pushd ..
		for %%f in (*.csproj) do (
			echo Building %%f
			msbuild .\%%f /p:Configuration="Release" /p:RELEASE="Release" /p:OutputPath="bin\Release" /p:Platform="Any CPU" /t:Rebuild 
		)
	popd
popd
