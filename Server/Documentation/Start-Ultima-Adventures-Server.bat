@echo off
setlocal

set "SERVER_ROOT=%~dp0.."
for %%I in ("%SERVER_ROOT%") do set "SERVER_ROOT=%%~fI"
set "SERVER_EXE=%SERVER_ROOT%\Adventures Server.exe"

if not exist "%SERVER_EXE%" (
    echo [DiscordBridge] Server executable not found:
    echo   "%SERVER_EXE%"
    exit /b 1
)

REM Prevent duplicate server instances.
tasklist /FI "IMAGENAME eq Adventures Server.exe" | find /I "Adventures Server.exe" >NUL
if %ERRORLEVEL%==0 (
    echo [DiscordBridge] Adventures Server.exe is already running.
    exit /b 0
)

echo [Ultima Adventures] Launching "%SERVER_EXE%"...
start "Ultima Adventures Server" /D "%SERVER_ROOT%" "%SERVER_EXE%"
exit /b 0
