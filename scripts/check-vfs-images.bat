@echo off
setlocal
cd /d "%~dp0.."
call run.bat --vfs "examples\vfs\minimal.csv" --script "examples\stage3-success.txt"
if errorlevel 1 exit /b 1
call run.bat --vfs "examples\vfs\several files.csv" --script "examples\stage3-success.txt"
if errorlevel 1 exit /b 1
call run.bat --vfs "examples\vfs\deep.csv" --script "examples\stage3-success.txt"
if errorlevel 1 exit /b 1
echo PASS: minimal, multiple files and deep VFS
exit /b 0
