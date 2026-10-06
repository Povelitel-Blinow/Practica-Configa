@echo off
setlocal
cd /d "%~dp0"
dotnet run --project tests\ShellEmulator.Tests
exit /b %errorlevel%
