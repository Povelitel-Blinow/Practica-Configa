@echo off
setlocal
cd /d "%~dp0.."
call run.bat < examples\exit.txt
if errorlevel 1 exit /b 1
call run.bat --vfs "examples\vfs\several files.csv" < examples\exit.txt
if errorlevel 1 exit /b 1
call run.bat --script "examples\startup success.txt"
if errorlevel 1 exit /b 1
call run.bat --vfs "examples\vfs\several files.csv" --script "examples\startup success.txt"
if errorlevel 1 exit /b 1
call run.bat --script "examples\startup success.txt" --vfs "examples\vfs\several files.csv"
if errorlevel 1 exit /b 1
echo PASS: valid startup options
exit /b 0
