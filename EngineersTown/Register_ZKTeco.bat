@echo off
chcp 65001 >nul
echo ========================================
echo    ZKTeco SDK Installation Script
echo ========================================
echo.

:: Check if running as administrator
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo ERROR: Please run this script as Administrator!
    echo Right-click -> Run as administrator
    pause
    exit /b 1
)

echo Copying DLLs to system32...
xcopy /y "%~dp0ZKTeco_SDK\*.dll" "C:\Windows\System32\" >nul

echo Registering COM components...
regsvr32 /s "C:\Windows\System32\zkemkeeper.dll"
regsvr32 /s "C:\Windows\System32\plcommpro.dll"
regsvr32 /s "C:\Windows\System32\comms.dll"

echo.
echo [OK] ZKTeco SDK installed successfully!
echo.
echo Please restart your application.
echo.
pause
