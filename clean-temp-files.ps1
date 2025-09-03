# PowerShell script to clean temporary files
# Run this script when you encounter temporary file compilation errors

Write-Host "🧹 Cleaning temporary files..." -ForegroundColor Green

# Clean obj directory
if (Test-Path "obj") {
    Write-Host "Removing obj directory..." -ForegroundColor Yellow
    Remove-Item -Recurse -Force "obj" -ErrorAction SilentlyContinue
}

# Clean bin directory (if not running)
if (Test-Path "bin") {
    Write-Host "Removing bin directory..." -ForegroundColor Yellow
    Remove-Item -Recurse -Force "bin" -ErrorAction SilentlyContinue
}

# Clean Visual Studio temporary files
Write-Host "Cleaning Visual Studio temporary files..." -ForegroundColor Yellow
Get-ChildItem -Path . -Recurse -Name "*_vctmp*" -ErrorAction SilentlyContinue | ForEach-Object {
    Write-Host "Removing: $_" -ForegroundColor Red
    Remove-Item -Force $_ -ErrorAction SilentlyContinue
}

# Clean TFSTemp directories
Get-ChildItem -Path . -Recurse -Directory -Name "*TFSTemp*" -ErrorAction SilentlyContinue | ForEach-Object {
    Write-Host "Removing directory: $_" -ForegroundColor Red
    Remove-Item -Recurse -Force $_ -ErrorAction SilentlyContinue
}

# Clean .vs directory
if (Test-Path ".vs") {
    Write-Host "Removing .vs directory..." -ForegroundColor Yellow
    Remove-Item -Recurse -Force ".vs" -ErrorAction SilentlyContinue
}

Write-Host "✅ Cleanup completed!" -ForegroundColor Green
Write-Host "Now run: dotnet build" -ForegroundColor Cyan
