@echo off
setlocal
cd /d "%~dp0.."
call run.bat --vfs
if not errorlevel 1 exit /b 1
call run.bat --script
if not errorlevel 1 exit /b 1
call run.bat --unknown value
if not errorlevel 1 exit /b 1
call run.bat --vfs first --vfs second
if not errorlevel 1 exit /b 1
call run.bat --script "examples\missing.txt"
if not errorlevel 1 exit /b 1
call run.bat --vfs "examples\vfs\several files.csv" --script "examples\startup-error.txt"
if not errorlevel 1 exit /b 1
echo PASS: invalid startup options and script failure
exit /b 0
