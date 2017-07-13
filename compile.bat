@ECHO OFF
SETLOCAL EnableDelayedExpansion

:: msbuild path
SET vsInstallerRoot=%programfiles(x86)%\Microsoft Visual Studio\Installer
FOR /f "usebackq tokens=1* delims=: " %%i IN (`"%vsInstallerRoot%\vswhere.exe" -latest -prerelease -requires Microsoft.Component.MSBuild`) DO (
  IF /i "%%i"=="installationPath" SET vsRoot=%%j
)

SET MSBuild="%vsRoot%\MSBuild\15.0\Bin\MSBuild.exe"

:: msbuild params
SET MSBuildParam=
SET MSBuildParam=%MSBuildParam% /maxcpucount
SET MSBuildParam=%MSBuildParam% /nologo
SET MSBuildParam=%MSBuildParam% /nodeReuse:false
SET MSBuildParam=%MSBuildParam% /property:AllowedReferenceRelatedFileExtensions=none
SET MSBuildParam=%MSBuildParam% /property:Configuration=Release
SET MSBuildParam=%MSBuildParam% /target:clean,restore,build
SET MSBuildParam=%MSBuildParam% /verbosity:minimal

:: execute msbuild
FOR %%a IN ("%~dp0\*.sln") DO (
    %MSBuild% %MSBuildParam% %%~fa
    IF NOT %errorlevel%==0 PAUSE
)

ENDLOCAL
EXIT /b