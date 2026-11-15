@echo off
REM
REM -- RunUO compile script for .NET 4.0 --
REM
REM The full .NET framework needs to be installed for this script.
REM i.e. not the "Client Profile", as it is missing several required DLLs.
REM
set targetfile=Adventures Server.exe

if exist "%targetfile%" (
	echo Deleting binary...
	del "%targetfile%" 1>NUL 2>NUL
	
	if exist "%targetfile%" (
		echo Failed!
		echo.
		echo Is "%targetfile%" in use?
		echo.
		goto end
	) else (
		echo Success.
		echo.
	)
)

REM MSBuild/dotnet may leave generated framework attributes under obj folders.
REM The legacy recursive core and script compilers treat those files as source.
if exist "Server\obj" (
	for /r "Server\obj" %%F in (*.AssemblyAttributes.cs) do del /q "%%F" 1>NUL 2>NUL
)
if exist "Scripts\obj" (
	for /r "Scripts\obj" %%F in (*.AssemblyAttributes.cs) do del /q "%%F" 1>NUL 2>NUL
)

echo Recompiling...
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /unsafe /define:NETFLIGHT_DIAGNOSTICS /out:"%targetfile%" /recurse:Server\*.cs /win32icon:Server\runuo.ico /optimize /main:Server.Core

:end
pause
