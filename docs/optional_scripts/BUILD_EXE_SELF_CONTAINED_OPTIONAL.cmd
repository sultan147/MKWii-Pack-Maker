@echo off
setlocal EnableExtensions EnableDelayedExpansion
cd /d "%~dp0..\.."

echo MKWii Pack Maker optional self-contained build script
echo ======================================================
echo.

where dotnet >nul 2>nul
if errorlevel 1 (
  echo ERROR: .NET SDK was not found.
  pause
  exit /b 1
)

if exist "MKWiiPackMaker.exe" del /f /q "MKWiiPackMaker.exe"
if exist "build_publish" rmdir /s /q "build_publish"

echo Publishing larger self-contained Windows EXE...
dotnet publish "src\MKWiiPackMaker.csproj" -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true /p:EnableCompressionInSingleFile=true -o "build_publish"
if errorlevel 1 (
  echo ERROR: Build failed.
  pause
  exit /b 1
)
copy /y "build_publish\MKWiiPackMaker.exe" "MKWiiPackMaker.exe" >nul

echo.
echo Self-contained build complete.
echo.
pause
