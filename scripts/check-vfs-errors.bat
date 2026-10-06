@echo off
setlocal
cd /d "%~dp0.."
call run.bat --vfs "examples\vfs\invalid-base64.csv" --script "examples\stage3-success.txt"
if not errorlevel 1 exit /b 1
call run.bat --vfs "examples\vfs\invalid-parent.csv" --script "examples\stage3-success.txt"
if not errorlevel 1 exit /b 1
call run.bat --vfs "examples\vfs\missing.csv"
if not errorlevel 1 exit /b 1
call run.bat --vfs "examples\vfs\deep.csv" --script "examples\startup-error.txt"
if not errorlevel 1 exit /b 1
call run.bat --vfs "examples\vfs\deep.csv" --script "examples\stage3-cd-error.txt"
if not errorlevel 1 exit /b 1
call run.bat --vfs "examples\vfs\deep.csv" --script "examples\stage3-exit-error.txt"
if not errorlevel 1 exit /b 1
call run.bat --vfs "examples\vfs\deep.csv" --script "examples\stage3-quote-error.txt"
if not errorlevel 1 exit /b 1
echo PASS: invalid VFS and fail-fast startup commands
exit /b 0
