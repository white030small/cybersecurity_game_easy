@echo off
cd /d "%~dp0"

echo === Build script started ===
echo Current directory: %cd%
echo.
echo Press any key to begin compile...
pause > nul

echo.
echo Looking for csc.exe...
set CSC=
if exist "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" set CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe
if "%CSC%"=="" if exist "C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe" set CSC=C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe
if "%CSC%"=="" if exist "C:\Windows\Microsoft.NET\Framework64\v3.5\csc.exe" set CSC=C:\Windows\Microsoft.NET\Framework64\v3.5\csc.exe
if "%CSC%"=="" if exist "C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe" set CSC=C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe

if "%CSC%"=="" (
    echo.
    echo [ERROR] csc.exe NOT FOUND
    echo.
    echo --- Framework64 folder ---
    dir "C:\Windows\Microsoft.NET\Framework64" 2>nul
    echo.
    echo --- Framework folder ---
    dir "C:\Windows\Microsoft.NET\Framework" 2>nul
    echo.
    echo Press any key to exit...
    pause > nul
    exit /b 1
)

echo Found: %CSC%
echo.
echo Compiling SecurityTestGame.cs...

"%CSC%" /nologo /target:winexe /out:SecurityTestGame.exe /reference:System.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll SecurityTestGame.cs

echo.
echo Compiler exit code: %errorlevel%
echo.

if exist "SecurityTestGame.exe" (
    echo [SUCCESS] SecurityTestGame.exe created
    echo Location: %cd%\SecurityTestGame.exe
) else (
    echo [FAILED] No EXE produced
)

echo.
echo Press any key to exit...
pause > nul
