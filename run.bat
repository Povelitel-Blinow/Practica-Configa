@echo off
setlocal
cd /d "%~dp0"
dotnet run --project src\ShellEmulator -- %*
exit /b %errorlevel%
