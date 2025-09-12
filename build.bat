@echo off
setlocal
echo Building ValheimVRM...

:: Root is the directory of this script
set "ROOT=%~dp0"
set "UNIVRM_UNITY_LIBS=%ROOT%Libs"
set "PROJECT_DIR=%ROOT%ValheimVRM"
set "OUT_DLL=%PROJECT_DIR%\bin\Release\net48\ValheimVRM.dll"

:: Go to project directory
cd /d "%PROJECT_DIR%"

:: Build the project
dotnet build --configuration Release
if errorlevel 1 (
    exit /b 1
)

endlocal
