@echo off
setlocal
echo Building ValheimVRM...

:: Root is the directory of this script
set "ROOT=%~dp0"
set "UNIVRM_UNITY_LIBS=%ROOT%Libs"

cd /d "%ROOT%"
dotnet build -c Release
if errorlevel 1 (
    exit /b 1
)

endlocal
