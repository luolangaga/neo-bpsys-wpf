@echo off
:: switch dir
cd /d %~dp0

:: build csporj
set BUILD_PATH=".\build\neo-bpsys-wpf"
set PROJ_PATH=".\neo-bpsys-wpf\neo-bpsys-wpf.csproj"

:: check output dir
if exist %BUILD_PATH% (
	rmdir /s /q %BUILD_PATH%
)
mkdir %BUILD_PATH%
:: build (self-contained, win-x64)
dotnet publish %PROJ_PATH% -c Release -r win-x64 --self-contained true ^
	/p:PublishSingleFile=false /p:PublishTrimmed=false -o %BUILD_PATH%

:: pack installer
:: set packer dir
set ISCC_PATH=".\InstallerGenerate\Inno Setup 6\ISCC.exe"
:: set pack script dir
set INSTALLER_PATH=".\InstallerGenerate\build_Installer.iss"
:: pack
%ISCC_PATH% %INSTALLER_PATH%