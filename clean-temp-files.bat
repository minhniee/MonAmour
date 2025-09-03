@echo off
echo 🧹 Cleaning temporary files...

REM Clean obj directory
if exist "obj" (
    echo Removing obj directory...
    rmdir /s /q "obj" 2>nul
)

REM Clean bin directory
if exist "bin" (
    echo Removing bin directory...
    rmdir /s /q "bin" 2>nul
)

REM Clean .vs directory
if exist ".vs" (
    echo Removing .vs directory...
    rmdir /s /q ".vs" 2>nul
)

REM Clean temporary files
echo Cleaning temporary files...
for /r %%i in (*_vctmp*.*) do (
    echo Removing: %%i
    del /f /q "%%i" 2>nul
)

echo ✅ Cleanup completed!
echo Now run: dotnet build
pause
