@echo off
setlocal
cd /d "%~dp0.."
call run.bat --vfs "examples\vfs\stage4.csv" --script "examples\stage4-success.txt"
if errorlevel 1 exit /b 1
call run.bat --vfs "examples\vfs\stage4.csv" --script "examples\stage4-stdin.txt" < "examples\rev-input.txt"
if errorlevel 1 exit /b 1
call run.bat --vfs "examples\vfs\stage4.csv" --script "examples\stage4-ls-missing.txt"
if not errorlevel 1 exit /b 1
call run.bat --vfs "examples\vfs\stage4.csv" --script "examples\stage4-ls-empty.txt"
if not errorlevel 1 exit /b 1
call run.bat --vfs "examples\vfs\stage4.csv" --script "examples\stage4-cd-file.txt"
if not errorlevel 1 exit /b 1
call run.bat --vfs "examples\vfs\stage4.csv" --script "examples\stage4-cd-missing.txt"
if not errorlevel 1 exit /b 1
call run.bat --vfs "examples\vfs\stage4.csv" --script "examples\stage4-cd-args.txt"
if not errorlevel 1 exit /b 1
call run.bat --vfs "examples\vfs\stage4.csv" --script "examples\stage4-find-args.txt"
if not errorlevel 1 exit /b 1
call run.bat --vfs "examples\vfs\stage4.csv" --script "examples\stage4-find-missing.txt"
if not errorlevel 1 exit /b 1
call run.bat --vfs "examples\vfs\stage4.csv" --script "examples\stage4-rev-directory.txt"
if not errorlevel 1 exit /b 1
call run.bat --vfs "examples\vfs\stage4.csv" --script "examples\stage4-rev-missing.txt"
if not errorlevel 1 exit /b 1
call run.bat --vfs "examples\vfs\stage4.csv" --script "examples\stage4-rev-binary.txt"
if not errorlevel 1 exit /b 1
call run.bat --vfs "examples\vfs\stage4.csv" --script "examples\stage4-path-file-parent.txt"
if not errorlevel 1 exit /b 1
echo PASS: stage 4 commands and errors
exit /b 0
